// ApiClient/YahooFinanceClient.cs
using DTOs;
using System.Text.Json;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace ApiClient
{
    public class YahooFinanceClient
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public YahooFinanceClient(HttpClient http, IMemoryCache cache)
        {
            _http = http; _cache = cache;
        }

        public async Task<List<QuoteDTO>> GetQuotesAsync(IEnumerable<string> symbols, CancellationToken ct = default)
        {
            var symList = symbols?
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim().ToUpperInvariant())
                .Distinct()
                .ToList() ?? new();

            if (symList.Count == 0) return new();

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

            await _lock.WaitAsync(ct);
            try
            {
                var remaining = new List<string>();
                foreach (var s in toFetch)
                {
                    if (!_cache.TryGetValue($"y_{s}", out QuoteDTO? _)) remaining.Add(s);
                }
                if (remaining.Count == 0) return result;

                var joined = string.Join(",", remaining);
                var url = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={joined}";

                var attempts = 0;
                while (true)
                {
                    attempts++;
                    using var resp = await _http.GetAsync(url, ct);

                    if (resp.StatusCode == HttpStatusCode.TooManyRequests || (int)resp.StatusCode >= 500)
                    {
                        if (attempts >= 3) break;
                        var baseMs = 400 + Random.Shared.Next(0, 250); // jitter
                        var delay = baseMs * attempts * attempts;
                        if (resp.Headers.RetryAfter?.Delta is { } delta)
                            delay = Math.Max(delay, (int)delta.TotalMilliseconds);
                        await Task.Delay(delay, ct);
                        continue;
                    }

                    if (!resp.IsSuccessStatusCode) break;

                    var json = await resp.Content.ReadAsStringAsync(ct);
                    using var doc = JsonDocument.Parse(json);
                    var results = doc.RootElement.GetProperty("quoteResponse").GetProperty("result");

                    var fetched = new List<QuoteDTO>();
                    foreach (var el in results.EnumerateArray())
                    {
                        var sym = el.GetProperty("symbol").GetString() ?? "";
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
                        fetched.Add(q);
                        _cache.Set($"y_{sym}", q, TimeSpan.FromSeconds(60));
                    }

                    result.AddRange(fetched);
                    break;
                }
            }
            finally { _lock.Release(); }

            return result;
        }
    }
}
