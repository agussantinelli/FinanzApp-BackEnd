using DTOs;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Globalization;


namespace ApiClient
{
    public class BinanceClient
    {
        private readonly HttpClient _http;
        public BinanceClient(HttpClient http) => _http = http;

        public async Task<List<QuoteDTO>> GetSpotPricesAsync(IEnumerable<string> symbols, CancellationToken ct = default)
        {
            var list = symbols?
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim().ToUpperInvariant())
                .Distinct()
                .ToList() ?? new();

            if (list.Count == 0) return new();

            // endpoint batch de Binance
            var payload = JsonSerializer.Serialize(list);
            var url = $"https://api.binance.com/api/v3/ticker/price?symbols={Uri.EscapeDataString(payload)}";

            var attempts = 0;
            while (true)
            {
                attempts++;
                using var resp = await _http.GetAsync(url, ct);

                if (resp.StatusCode == HttpStatusCode.TooManyRequests || (int)resp.StatusCode >= 500)
                {
                    if (attempts >= 3) return new(); // devolver vacío sin romper
                    var delayMs = 700 * attempts;
                    if (resp.Headers.RetryAfter?.Delta is { } delta) delayMs = (int)delta.TotalMilliseconds;
                    await Task.Delay(Math.Max(400, delayMs), ct);
                    continue;
                }

                if (!resp.IsSuccessStatusCode) return new();

                var json = await resp.Content.ReadAsStringAsync(ct);
                var arr = JsonDocument.Parse(json).RootElement;

                var outList = new List<QuoteDTO>();
                foreach (var el in arr.EnumerateArray())
                {
                    var sym = el.GetProperty("symbol").GetString()!;
                    var px = el.GetProperty("price").GetString();
                    if (decimal.TryParse(px, NumberStyles.Number, CultureInfo.InvariantCulture, out var d))
                    {
                        outList.Add(new QuoteDTO
                        {
                            Symbol = sym,
                            Price = d,    
                            Currency = "USD",
                            Source = "Binance",
                            TimestampUtc = DateTime.UtcNow
                        });
                    }
                }
                return outList;
            }
        }
    }
}
