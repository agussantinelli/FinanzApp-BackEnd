using ApiClient;
using DTOs;

namespace Services
{
    public class StocksService
    {
        private readonly YahooFinanceClient _yahoo;
        private readonly DolarService _dolar;

        public StocksService(YahooFinanceClient yahoo, DolarService dolar)
        {
            _yahoo = yahoo;
            _dolar = dolar;
        }

        public async Task<List<DualQuoteDTO>> GetDualsAsync(
            (string localBA, string usa)[] pairs,
            string dolarPreferido = "CCL",
            CancellationToken ct = default)
        {
            var cleanPairs = (pairs ?? Array.Empty<(string, string)>())
                .Select(p => (localBA: (p.localBA ?? "").Trim().ToUpperInvariant(),
                              usa: (p.usa ?? "").Trim().ToUpperInvariant()))
                .Where(p => !string.IsNullOrWhiteSpace(p.localBA) && !string.IsNullOrWhiteSpace(p.usa))
                .Distinct()
                .ToArray();

            if (cleanPairs.Length == 0) return new();

            var tc = await _dolar.GetTcAsync(dolarPreferido, ct);
            if (tc <= 0m) return new();

            var allSymbols = cleanPairs.Select(p => p.localBA)
                                       .Concat(cleanPairs.Select(p => p.usa))
                                       .Distinct(StringComparer.OrdinalIgnoreCase)
                                       .ToList();

            var quotes = await _yahoo.GetQuotesAsync(allSymbols, ct);
            if (quotes.Count == 0) return new();

            var bySymbol = quotes.ToDictionary(q => q.Symbol, StringComparer.OrdinalIgnoreCase);

            var result = new List<DualQuoteDTO>(cleanPairs.Length);
            foreach (var (localBA, usa) in cleanPairs)
            {
                if (!bySymbol.TryGetValue(localBA, out var lq)) continue;
                if (!bySymbol.TryGetValue(usa, out var uq)) continue;

                result.Add(new DualQuoteDTO
                {
                    LocalSymbol = lq.Symbol,
                    LocalPriceARS = lq.Price,
                    UsSymbol = uq.Symbol,
                    UsPriceUSD = uq.Price,
                    UsedDollarRate = tc,
                    DollarRateName = dolarPreferido
                });
            }

            return result;
        }
    }
}
