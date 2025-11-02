// Services/StocksService.cs
using ApiClient;
using DTOs;

namespace Services;

public sealed class StocksService
{
    private readonly YahooFinanceClient _yahoo;
    private readonly DolarService _dolar;

    public StocksService(YahooFinanceClient yahoo, DolarService dolar)
    {
        _yahoo = yahoo;
        _dolar = dolar;
    }

    public async Task<List<DualQuoteDTO>> GetDualsAsync(
        (string localBA, string usa, decimal? cedearRatio)[] pairs,
        string dolarPreferido,
        CancellationToken ct = default)
    {
        // 1) TC
        var (tc, tcName) = await _dolar.GetTcAsync(dolarPreferido, ct);

        // 2) Símbolos a consultar
        var allSymbols = pairs
            .SelectMany(p => new[] { p.localBA, p.usa })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var quotes = await _yahoo.GetQuotesAsync(allSymbols, ct);

        // 3) Armar salida
        var list = new List<DualQuoteDTO>();
        foreach (var (localBA, usa, ratio) in pairs)
        {
            quotes.TryGetValue(localBA, out var localArs);
            quotes.TryGetValue(usa, out var usUsd);

            list.Add(new DualQuoteDTO
            {
                LocalSymbol = localBA,
                LocalPriceARS = localArs,
                UsSymbol = usa,
                UsPriceUSD = usUsd,
                CedearRatio = ratio,
                UsedDollarRate = tc,
                DollarRateName = tcName
            });
        }
        return list;
    }
}
