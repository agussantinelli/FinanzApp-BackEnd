using ApiClient;
using DTOs;

namespace Services
{
    public class CedearsService
    {
        private readonly YahooFinanceClient _yahoo;
        private readonly DolarService _dolar;

        // Mapa rápido de ratios (completar/ajustar con tabla oficial)
        private static readonly List<CedearRatioDTO> Ratios = new()
        {
            new() { CedearSymbol = "AAPL.BA", UsSymbol = "AAPL", Ratio = 10m },
            new() { CedearSymbol = "AMZN.BA", UsSymbol = "AMZN", Ratio = 36m },
            new() { CedearSymbol = "TSLA.BA", UsSymbol = "TSLA", Ratio = 15m },
        };

        public CedearsService(YahooFinanceClient yahoo, DolarService dolar)
        {
            _yahoo = yahoo; _dolar = dolar;
        }

        public async Task<List<DualQuoteDTO>> GetCedearDualsAsync(string dolarPreferido = "CCL")
        {
            var dolar = (await _dolar.GetCotizacionesAsync())
                        .FirstOrDefault(d => d.Nombre.Equals(dolarPreferido, StringComparison.OrdinalIgnoreCase));
            var tc = (decimal?)dolar?.Venta ?? 0m;

            var cedearSyms = Ratios.Select(r => r.CedearSymbol);
            var usSyms = Ratios.Select(r => r.UsSymbol);

            var cedearQuotes = await _yahoo.GetQuotesAsync(cedearSyms);
            var usQuotes = await _yahoo.GetQuotesAsync(usSyms);

            var list = new List<DualQuoteDTO>();
            foreach (var r in Ratios)
            {
                var cq = cedearQuotes.FirstOrDefault(q => q.Symbol == r.CedearSymbol);
                var uq = usQuotes.FirstOrDefault(q => q.Symbol == r.UsSymbol);
                if (cq is null || uq is null || tc == 0) continue;

                list.Add(new DualQuoteDTO
                {
                    LocalSymbol = cq.Symbol,
                    LocalPriceARS = cq.Price,   // CEDEAR cotiza en ARS
                    UsSymbol = uq.Symbol,
                    UsPriceUSD = uq.Price,
                    UsedDollarRate = tc,
                    DollarRateName = dolarPreferido,
                    CedearRatio = r.Ratio
                });
            }
            return list;
        }
    }
}
