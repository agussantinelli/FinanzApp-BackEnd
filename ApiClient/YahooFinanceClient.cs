using DTOs;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ApiClient
{
    public class YahooFinanceClient
    {
        private readonly HttpClient _http;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public YahooFinanceClient(HttpClient http, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        {
            _http = http;
            _cache = cache;
        }

        private static string BuildUrl(IEnumerable<string> symbols)
        {
            var joined = string.Join(",", symbols);
            var enc = Uri.EscapeDataString(joined);
            return $"https://query2.finance.yahoo.com/v7/finance/quote?symbols={enc}";
        }

        public async Task<List<QuoteDTO>> GetQuotesAsync(IEnumerable<string> symbols, CancellationToken ct = default)
        {
            var symList = symbols?
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim().ToUpperInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new();

            if (symList.Count == 0) return new();

            // Cache hit rápido
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

            var chunks = toFetch.Chunk(50).Select(c => c.ToList()).ToList();

            await _lock.WaitAsync(ct);
            try
            {
                foreach (var chunk in chunks)
                {
                    // re-check por si otro hilo ya cacheó
                    var remaining = new List<string>();
                    foreach (var s in chunk)
                        if (!_cache.TryGetValue($"y_{s}", out QuoteDTO? _)) remaining.Add(s);
                    if (remaining.Count == 0) continue;

                    var url = BuildUrl(remaining);

                    var attempts = 0;
                    while (true)
                    {
                        attempts++;

                        using var req = new HttpRequestMessage(HttpMethod.Get, url);
                        // HEADERS CRÍTICOS para evitar 401 anónimo
                        req.Headers.TryAddWithoutValidation("User-Agent",
                            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                        req.Headers.TryAddWithoutValidation("Accept", "application/json,text/plain,*/*");
                        req.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9,es-AR;q=0.8");
                        req.Headers.TryAddWithoutValidation("Connection", "keep-alive");

                        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

                        // Si da 401, probamos una vez alternando a query1
                        if (resp.StatusCode == HttpStatusCode.Unauthorized && attempts == 1)
                        {
                            url = url.Replace("query2.", "query1.");
                            await Task.Delay(300, ct);
                            continue;
                        }

                        // Backoff por rate limit/errores 5xx
                        if (resp.StatusCode == (HttpStatusCode)429 || (int)resp.StatusCode >= 500)
                        {
                            if (attempts >= 3) break;
                            var baseMs = 500 + Random.Shared.Next(0, 300);
                            var delay = baseMs * attempts * attempts;
                            if (resp.Headers.RetryAfter?.Delta is { } delta)
                                delay = Math.Max(delay, (int)delta.TotalMilliseconds);
                            await Task.Delay(delay, ct);
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
                            if (!el.TryGetProperty("regularMarketPrice", out var p)) continue;

                            var px = p.GetDecimal();
                            var cur = el.TryGetProperty("currency", out var c) ? (c.GetString() ?? "USD") : "USD";

                            var q = new QuoteDTO
                            {
                                Symbol = sym,
                                Price = Math.Round(px, 2),
                                Currency = cur,
                                Source = "YahooFinance",
                                TimestampUtc = DateTime.UtcNow
                            };
                            _cache.Set($"y_{sym}", q, TimeSpan.FromSeconds(60));
                            result.Add(q);
                        }
                        break;
                    }
                }
            }
            finally { _lock.Release(); }

            return result;
        }
    }
}
