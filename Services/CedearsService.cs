using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiClient;
using DTOs;

namespace Services
{
    public class CedearsService
    {
        private readonly YahooFinanceClient _yahoo;
        private readonly DolarService _dolar;

        // Ratios de referencia (completá/ajustá según lista vigente).
        // Significa: <Ratio> CEDEARs = 1 acción USA.
        private static readonly List<CedearRatioDTO> Ratios = new()
        {
            new() { CedearSymbol = "AAPL.BA", UsSymbol = "AAPL", Ratio = 10m },
            new() { CedearSymbol = "AMZN.BA", UsSymbol = "AMZN", Ratio = 36m },
            new() { CedearSymbol = "TSLA.BA", UsSymbol = "TSLA", Ratio = 15m },
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

            var dolar = (await _dolar.GetCotizacionesAsync()).FirstOrDefault(d =>
                d.Nombre.Equals(dolarPreferido, StringComparison.OrdinalIgnoreCase));
            var tc = (decimal?)dolar?.Venta ?? 0m;
            if (tc <= 0m) return new();

            // 1Armar una sola lista de símbolos (cedear + usa) y pedirlos juntos
            var cedearSyms = Ratios.Select(r => (r.CedearSymbol ?? "").Trim().ToUpperInvariant())
                                   .Where(s => !string.IsNullOrWhiteSpace(s));
            var usSyms = Ratios.Select(r => (r.UsSymbol ?? "").Trim().ToUpperInvariant())
                                   .Where(s => !string.IsNullOrWhiteSpace(s));

            var all = cedearSyms.Concat(usSyms)
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToList();

            if (all.Count == 0) return new();

            var quotes = await _yahoo.GetQuotesAsync(all, ct);
            if (quotes.Count == 0) return new();

            var bySymbol = quotes.ToDictionary(q => q.Symbol, StringComparer.OrdinalIgnoreCase);

            // Armar respuesta
            var list = new List<DualQuoteDTO>(Ratios.Count);
            foreach (var r in Ratios)
            {
                if (!bySymbol.TryGetValue(r.CedearSymbol, out var cq)) continue; // CEDEAR (ARS)
                if (!bySymbol.TryGetValue(r.UsSymbol, out var uq)) continue;     // USA (USD)
                if (r.Ratio <= 0m) continue;

                var dto = new DualQuoteDTO
                {
                    LocalSymbol = cq.Symbol,
                    LocalPriceARS = cq.Price, // CEDEAR en ARS
                    UsSymbol = uq.Symbol,
                    UsPriceUSD = uq.Price, // Acción USA en USD
                    UsedDollarRate = tc,
                    DollarRateName = dolarPreferido,
                    CedearRatio = r.Ratio
                };

                list.Add(dto);
            }

            return list;
        }
    }
}
