using DTOs;
using System.Text.Json;

namespace ApiClient
{
    public class CoinGeckoClient
    {
        private readonly HttpClient _http;
        public CoinGeckoClient(HttpClient http) => _http = http;

        public async Task<List<CryptoTopDTO>> GetTopAsync(int limit = 10, CancellationToken ct = default)
        {
            limit = Math.Clamp(limit, 1, 50);
            var url = $"https://api.coingecko.com/api/v3/coins/markets" +
                      $"?vs_currency=usd&order=market_cap_desc&per_page={limit}&page=1&price_change_percentage=24h";

            using var resp = await _http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return new();

            var json = await resp.Content.ReadAsStringAsync(ct);
            var arr = JsonDocument.Parse(json).RootElement;

            var list = new List<CryptoTopDTO>();
            foreach (var el in arr.EnumerateArray())
            {
                list.Add(new CryptoTopDTO
                {
                    Rank = el.GetProperty("market_cap_rank").GetInt32(),
                    Name = el.GetProperty("name").GetString() ?? "",
                    Symbol = (el.GetProperty("symbol").GetString() ?? "").ToUpperInvariant(),
                    PriceUsd = Math.Round(el.GetProperty("current_price").GetDecimal(), 2),
                    Source = "CoinGecko",
                    TimestampUtc = DateTime.UtcNow
                });
            }
            return list;
        }
    }
}
