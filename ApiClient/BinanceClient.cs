using DTOs;
using System.Net.Http.Json;

namespace ApiClient
{
    public class BinanceClient
    {
        private readonly HttpClient _http;
        public BinanceClient(HttpClient http) => _http = http;

        public async Task<List<QuoteDTO>> GetSpotPricesAsync(IEnumerable<string> symbols)
        {
            var list = new List<QuoteDTO>();
            foreach (var s in symbols)
            {
                var resp = await _http.GetFromJsonAsync<BinancePrice>($"https://api.binance.com/api/v3/ticker/price?symbol={s}");
                if (resp is not null && decimal.TryParse(resp.price, out var px))
                {
                    list.Add(new QuoteDTO
                    {
                        Symbol = s,
                        Price = px,
                        Currency = "USD",
                        Source = "Binance"
                    });
                }
            }
            return list;
        }

        private record BinancePrice(string symbol, string price);
    }
}
