using DTOs;
using System.Text.Json;

namespace ApiClient
{
    public class YahooFinanceClient
    {
        private readonly HttpClient _http;
        public YahooFinanceClient(HttpClient http) => _http = http;

        public async Task<List<QuoteDTO>> GetQuotesAsync(IEnumerable<string> symbols)
        {
            var joined = string.Join(",", symbols);
            var url = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={joined}";
            using var resp = await _http.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var results = doc.RootElement
                .GetProperty("quoteResponse")
                .GetProperty("result");

            var list = new List<QuoteDTO>();
            foreach (var el in results.EnumerateArray())
            {
                var sym = el.GetProperty("symbol").GetString() ?? "";
                var px = el.TryGetProperty("regularMarketPrice", out var p) ? p.GetDecimal() : 0m;
                var cur = el.TryGetProperty("currency", out var c) ? (c.GetString() ?? "USD") : "USD";

                list.Add(new QuoteDTO
                {
                    Symbol = sym,
                    Price = Math.Round(px, 2),
                    Currency = cur,
                    Source = "YahooFinance"
                });
            }
            return list;
        }
    }
}
