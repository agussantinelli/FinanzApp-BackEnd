using DTOs;
using System.Text.Json;

namespace ApiClient
{
    public class CoinCapClient
    {
        private readonly HttpClient _http;
        public CoinCapClient(HttpClient http) => _http = http;

        public async Task<List<CryptoTopDTO>> GetTopAsync(int limit = 10, CancellationToken ct = default)
        {
            var url = $"https://api.coincap.io/v2/assets?limit={Math.Clamp(limit, 1, 50)}";
            using var resp = await _http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return new();

            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var arr = doc.RootElement.GetProperty("data");

            var list = new List<CryptoTopDTO>();
            foreach (var el in arr.EnumerateArray())
            {
                list.Add(new CryptoTopDTO
                {
                    Rank = int.Parse(el.GetProperty("rank").GetString() ?? "0"),
                    Name = el.GetProperty("name").GetString() ?? "",
                    Symbol = (el.GetProperty("symbol").GetString() ?? "").ToUpperInvariant(),
                    PriceUsd = Math.Round(decimal.Parse(el.GetProperty("priceUsd").GetString()!, System.Globalization.CultureInfo.InvariantCulture), 2),
                    Source = "CoinCap",
                    TimestampUtc = DateTime.UtcNow
                });
            }
            return list;
        }
    }
}
