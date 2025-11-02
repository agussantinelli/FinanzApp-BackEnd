// Services/CedearsService.cs
using ApiClient;
using DTOs;
using Microsoft.Extensions.Logging;

namespace Services;

public sealed class CedearsService
{
    private readonly YahooFinanceClient _yahoo;
    private readonly DolarService _dolar;
    private readonly ILogger<CedearsService> _log;

    public CedearsService(YahooFinanceClient yahoo, DolarService dolar, ILogger<CedearsService> log)
    {
        _yahoo = yahoo;
        _dolar = dolar;
        _log = log;
    }

    public sealed record CedearReq(string cedearSymbol, string usSymbol, decimal? ratio);

    public async Task<List<DualQuoteDTO>> GetCedearQuotesAsync(
        IEnumerable<CedearReq> reqs,
        string dolarPreferido,
        CancellationToken ct = default)
    {
        var (tc, tcName) = await _dolar.GetTcAsync(dolarPreferido, ct);

        var symbols = reqs
            .SelectMany(r => new[] { r.cedearSymbol, r.usSymbol })
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var quotes = await _yahoo.GetQuotesAsync(symbols, ct);

        var list = new List<DualQuoteDTO>();
        foreach (var r in reqs)
        {
            quotes.TryGetValue(r.cedearSymbol, out var cedearArs);
            quotes.TryGetValue(r.usSymbol, out var usUsd);

            // ratio: primero el provisto, si no, intento catálogo interno (opcional)
            decimal? ratio = r.ratio;
            if (ratio is null && CedearsRatios.TryGetRatio(r.cedearSymbol, out var rx))
                ratio = rx;

            list.Add(new DualQuoteDTO
            {
                LocalSymbol = r.cedearSymbol,
                LocalPriceARS = cedearArs,
                UsSymbol = r.usSymbol,
                UsPriceUSD = usUsd,
                CedearRatio = ratio,
                UsedDollarRate = tc,        // <<< usar r.tc, no la tupla completa
                DollarRateName = tcName
            });
        }

        return list;
    }
}
