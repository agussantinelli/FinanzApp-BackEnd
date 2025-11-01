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
            _yahoo = yahoo; _dolar = dolar;
        }

        // Devuelve dualidad para pares: (local .BA, usa)
        public async Task<List<DualQuoteDTO>> GetDualsAsync((string localBA, string usa)[] pairs, string dolarPreferido = "CCL")
        {
            var localSymbols = pairs.Select(p => p.localBA);
            var usSymbols = pairs.Select(p => p.usa);

            var dolar = (await _dolar.GetCotizacionesAsync())
                        .FirstOrDefault(d => d.Nombre.Equals(dolarPreferido, StringComparison.OrdinalIgnoreCase));
            var tc = (decimal?)dolar?.Venta ?? 0m;

            var localQuotes = await _yahoo.GetQuotesAsync(localSymbols);
            var usQuotes = await _yahoo.GetQuotesAsync(usSymbols);

            var result = new List<DualQuoteDTO>();
            foreach (var (localBA, usa) in pairs)
            {
                var lq = localQuotes.FirstOrDefault(q => q.Symbol.Equals(localBA, StringComparison.OrdinalIgnoreCase));
                var uq = usQuotes.FirstOrDefault(q => q.Symbol.Equals(usa, StringComparison.OrdinalIgnoreCase));
                if (lq is null || uq is null || tc == 0) continue;

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
