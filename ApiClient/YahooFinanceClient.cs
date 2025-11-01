using DTOs;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ApiClient
{
    public class YahooFinanceClient
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;

        private static readonly SemaphoreSlim _lock = new(1, 1);
        private static volatile bool _primed = false;
        private static readonly object _primeSync = new();

        public YahooFinanceClient(HttpClient http, IMemoryCache cache)
        {
            _http = http;
            _cache = cache;

            _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json,text/plain,*/*");
            _http.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9,es-AR;q=0.8");
        }

        private async Task EnsurePrimedAsync(CancellationToken ct)
        {
            if (_primed) return;
            lock (_primeSync)
            {
                if (_primed) return;
                _primed = true;
            }
            using var req = new HttpRequestMessage(HttpMethod.Get, "https://finance.yahoo.com/");
            using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            _ = resp.Headers;
        }

        private static string BuildQuoteUrl(IEnumerable<string> syms, bool useQuery2 = true)
        {
            var joined = string.Join(",", syms);
            var enc = Uri.EscapeDataString(joined);
            var host = useQuery2 ? "query2" : "query1";
            return $"https://{host}.finance.yahoo.com/v7/finance/quote?symbols={enc}";
        }

        public async Task<List<QuoteDTO>> GetQuotesAsync(IEnumerable<string> symbols, CancellationToken ct = default)
        {
            var symList = symbols?
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim().ToUpperInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new();

            if (symList.Count == 0) return new();

            // Cache
            var toFetch = new List<string>();
            var result = new List<QuoteDTO>();
            foreach (var s in symList)
            {
                if (_cache.TryGetValue($"y_{s}", out QuoteDTO? q) && q is not null)
                    result.Add(q);
                else
                    toFetch.Add(s);
            }
            if (toFetch.Count == 0) return result;

            var chunks = toFetch.Chunk(50).Select(x => x.ToList()).ToList();

            await _lock.WaitAsync(ct);
            try
            {
                await EnsurePrimedAsync(ct);

                foreach (var chunk in chunks)
                {
                    var remaining = new List<string>();
                    foreach (var s in chunk)
                        if (!_cache.TryGetValue($"y_{s}", out QuoteDTO? _)) remaining.Add(s);
                    if (remaining.Count == 0) continue;

                    var apiFetched = await TryFetchFromApi(remaining, ct);

                    foreach (var q in apiFetched)
                        _cache.Set($"y_{q.Symbol}", q, TimeSpan.FromSeconds(60));
                    result.AddRange(apiFetched);

                    var missing = remaining.Where(s => !apiFetched.Any(q => q.Symbol.Equals(s, StringComparison.OrdinalIgnoreCase))).ToList();
                    if (missing.Count > 0)
                    {
                        var htmlFetched = new List<QuoteDTO>();
                        foreach (var sym in missing)
                        {
                            var q = await TryFetchFromHtml(sym, ct);
                            if (q is not null)
                            {
                                htmlFetched.Add(q);
                                _cache.Set($"y_{q.Symbol}", q, TimeSpan.FromSeconds(60));
                                await Task.Delay(120, ct);
                            }
                        }
                        result.AddRange(htmlFetched);
                    }
                }
            }
            finally
            {
                _lock.Release();
            }

            return result;
        }

        private async Task<List<QuoteDTO>> TryFetchFromApi(List<string> symbols, CancellationToken ct)
        {
            var list = new List<QuoteDTO>();
            if (symbols.Count == 0) return list;

            bool useQuery2 = true;
            var url = BuildQuoteUrl(symbols, useQuery2);

            for (int attempt = 1; attempt <= 2; attempt++)
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Referrer = new Uri("https://finance.yahoo.com/quote/AAPL");
                req.Headers.TryAddWithoutValidation("Origin", "https://finance.yahoo.com");

                using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    useQuery2 = !useQuery2;
                    url = BuildQuoteUrl(symbols, useQuery2);
                    continue;
                }

                if (!resp.IsSuccessStatusCode) break;

                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("quoteResponse", out var qResp) ||
                    !qResp.TryGetProperty("result", out var results))
                    break;

                foreach (var el in results.EnumerateArray())
                {
                    if (!el.TryGetProperty("symbol", out var symProp)) continue;
                    var sym = symProp.GetString() ?? "";

                    decimal px = 0;
                    if (el.TryGetProperty("regularMarketPrice", out var p)) px = p.GetDecimal();
                    else if (el.TryGetProperty("currentPrice", out var cp)) px = cp.GetDecimal();
                    if (px <= 0) continue;

                    var cur = el.TryGetProperty("currency", out var c) ? (c.GetString() ?? GuessCurrency(sym)) : GuessCurrency(sym);

                    list.Add(new QuoteDTO
                    {
                        Symbol = sym,
                        Price = Math.Round(px, 2),
                        Currency = cur,
                        Source = "YahooFinance",
                        TimestampUtc = DateTime.UtcNow
                    });
                }
                break;
            }

            return list;
        }

        private static readonly Regex RxPrice1 = new("\"regularMarketPrice\"\\s*:\\s*\\{\\s*\"raw\"\\s*:\\s*([0-9]+(?:\\.[0-9]+)?)", RegexOptions.Compiled);
        private static readonly Regex RxPrice2 = new("\"currentPrice\"\\s*:\\s*\\{\\s*\"raw\"\\s*:\\s*([0-9]+(?:\\.[0-9]+)?)", RegexOptions.Compiled);
        private static readonly Regex RxCurrency = new("\"currency\"\\s*:\\s*\"([A-Z]{3})\"", RegexOptions.Compiled);

        private async Task<QuoteDTO?> TryFetchFromHtml(string symbol, CancellationToken ct)
        {
            var url = $"https://finance.yahoo.com/quote/{Uri.EscapeDataString(symbol)}";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            req.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            req.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            req.Headers.TryAddWithoutValidation("Connection", "keep-alive");

            using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!resp.IsSuccessStatusCode) return null;

            var html = await resp.Content.ReadAsStringAsync(ct);

            decimal? price = null;
            var m1 = RxPrice1.Match(html);
            if (m1.Success && decimal.TryParse(m1.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p1))
                price = p1;
            else
            {
                var m2 = RxPrice2.Match(html);
                if (m2.Success && decimal.TryParse(m2.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p2))
                    price = p2;
            }

            if (price is null || price <= 0) return null;

            string currency = GuessCurrency(symbol);
            var mc = RxCurrency.Match(html);
            if (mc.Success)
            {
                var cur = mc.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(cur)) currency = cur;
            }

            return new QuoteDTO
            {
                Symbol = symbol,
                Price = Math.Round(price.Value, 2),
                Currency = currency,
                Source = "YahooFinance(HTML)",
                TimestampUtc = DateTime.UtcNow
            };
        }

        private static string GuessCurrency(string sym)
        {
            return sym.EndsWith(".BA", StringComparison.OrdinalIgnoreCase) ? "ARS" : "USD";
        }
    }
}
