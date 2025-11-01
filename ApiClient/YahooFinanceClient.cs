using DTOs;
using System.Text.Json;
using System.Net;

namespace ApiClient
{
    public class YahooFinanceClient
    {
        private readonly HttpClient _http;
        public YahooFinanceClient(HttpClient http) => _http = http;

        public async Task<List<QuoteDTO>> GetQuotesAsync(IEnumerable<string> symbols, CancellationToken ct = default)
        {
            // limpiar símbolos vacíos y duplicados
            var symList = symbols?
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new();

            if (symList.Count == 0) return new List<QuoteDTO>();

            // Yahoo tolera muchos símbolos, pero loteamos por prolijidad
            const int BATCH = 10;
            var result = new List<QuoteDTO>();

            for (int i = 0; i < symList.Count; i += BATCH)
            {
                var chunk = symList.Skip(i).Take(BATCH).ToList();
                var joined = string.Join(",", chunk);
                var url = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={joined}";

                // reintentos básicos para 429/5xx
                var attempts = 0;
                while (true)
                {
                    attempts++;
                    using var resp = await _http.GetAsync(url, ct);

                    if (resp.StatusCode == HttpStatusCode.TooManyRequests || (int)resp.StatusCode >= 500)
                    {
                        if (attempts >= 3) break; // dar por perdido este batch
                        // respetar Retry-After si vino
                        var delayMs = 700 * attempts;
                        if (resp.Headers.RetryAfter?.Delta is { } delta) delayMs = (int)delta.TotalMilliseconds;
                        await Task.Delay(Math.Max(400, delayMs), ct);
                        continue;
                    }

                    if (!resp.IsSuccessStatusCode) break; // pasar al siguiente batch sin explotar

                    var json = await resp.Content.ReadAsStringAsync(ct);
                    using var doc = JsonDocument.Parse(json);
                    var results = doc.RootElement
                        .GetProperty("quoteResponse")
                        .GetProperty("result");

                    foreach (var el in results.EnumerateArray())
                    {
                        var sym = el.GetProperty("symbol").GetString() ?? "";
                        var pxOk = el.TryGetProperty("regularMarketPrice", out var p);
                        var curOk = el.TryGetProperty("currency", out var c);

                        if (!pxOk) continue;
                        var px = p.GetDecimal();
                        var cur = curOk ? (c.GetString() ?? "USD") : "USD";

                        result.Add(new QuoteDTO
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
            }

            return result;
        }
    }
}
