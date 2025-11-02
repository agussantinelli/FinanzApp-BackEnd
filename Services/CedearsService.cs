using ApiClient;
using DTOs;

namespace Services
{

    public class CedearsService
    {
        private readonly YahooFinanceClient _yahoo;
        private readonly DolarService _dolar;

        private static readonly List<CedearRatioDTO> Ratios = new()
        {
            new() { CedearSymbol = "AAPL.BA",   UsSymbol = "AAPL",  Ratio = 10m },
            new() { CedearSymbol = "AMZN.BA",   UsSymbol = "AMZN",  Ratio = 36m },
            new() { CedearSymbol = "NVDA.BA",   UsSymbol = "NVDA",  Ratio = 20m }, 
            new() { CedearSymbol = "MSFT.BA",   UsSymbol = "MSFT",  Ratio = 5m  }, 
            new() { CedearSymbol = "GOOGL.BA",  UsSymbol = "GOOGL", Ratio = 5m  }, 
            new() { CedearSymbol = "META.BA",   UsSymbol = "META",  Ratio = 12m }, 
            new() { CedearSymbol = "TSLA.BA",   UsSymbol = "TSLA",  Ratio = 15m },
            new() { CedearSymbol = "BRKB.BA",   UsSymbol = "BRK-B", Ratio = 10m },
            new() { CedearSymbol = "KO.BA",     UsSymbol = "KO",    Ratio = 5m  },
        };

        public CedearsService(YahooFinanceClient yahoo, DolarService dolar)
        {
            _yahoo = yahoo;
            _dolar = dolar;
        }

        public async Task<List<DualQuoteDTO>> GetCedearDualsAsync(
            string dolarPreferido = "CCL",
            CancellationToken ct = default)
        {
            if (Ratios.Count == 0) return new();

            var tc = await _dolar.GetTcAsync(dolarPreferido, ct);
            if (tc <= 0m) return new();

            var all = Ratios.Select(r => r.CedearSymbol)
                            .Concat(Ratios.Select(r => r.UsSymbol))
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Select(s => s!.Trim().ToUpperInvariant())
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

            if (all.Count == 0) return new();

            var quotes = await _yahoo.GetQuotesAsync(all, ct);
            if (quotes.Count == 0) return new();

            var bySymbol = quotes.ToDictionary(q => q.Symbol, StringComparer.OrdinalIgnoreCase);

            var list = new List<DualQuoteDTO>(Ratios.Count);
            foreach (var r in Ratios)
            {
                if (r.Ratio <= 0m) continue;
                if (!bySymbol.TryGetValue(r.CedearSymbol, out var cq)) continue; // CEDEAR (ARS)
                if (!bySymbol.TryGetValue(r.UsSymbol, out var uq)) continue;     // USA (USD)

                list.Add(new DualQuoteDTO
                {
                    LocalSymbol = cq.Symbol,
                    LocalPriceARS = cq.Price,
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
