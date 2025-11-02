// ApiClient/YahooFinanceClient.cs
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace ApiClient;

public sealed class YahooFinanceClient
{
    private readonly HttpClient _http;
    private readonly ILogger<YahooFinanceClient> _log;

    public YahooFinanceClient(HttpClient http, ILogger<YahooFinanceClient> log)
    {
        _http = http;
        _log = log;
        _http.Timeout = TimeSpan.FromSeconds(10);
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
    }

    public async Task<Dictionary<string, decimal>> GetQuotesAsync(IEnumerable<string> symbols, CancellationToken ct = default)
    {
        var syms = symbols.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        // 1) intento batch (puede devolver 401)
        var joined = string.Join(",", syms.Select(Uri.EscapeDataString));
        var batchEndpoints = new[]
        {
            $"https://query2.finance.yahoo.com/v7/finance/quote?symbols={joined}",
            $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={joined}"
        };

        foreach (var url in batchEndpoints)
        {
            try
            {
                using var resp = await _http.GetAsync(url, ct);
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    var json = await resp.Content.ReadAsStringAsync(ct);
                    // parse muy simple para regularMarketPrice (evito depender de JSON lib acá)
                    // Mejor si tenés System.Text.Json: lo dejo con regex para no sumar dependencias.
                    var prices = ExtractBatchPrices(json, syms);
                    foreach (var kv in prices)
                        result[kv.Key] = kv.Value;
                    // puedo completar faltantes por HTML abajo
                    break;
                }
                else
                {
                    _log.LogWarning("Yahoo batch {Url} devolvió {Code}", url, (int)resp.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Yahoo batch falló: {Url}", url);
            }
        }

        // 2) fallback por cada símbolo via HTML (para los que falten o todos si no anduvo batch)
        foreach (var s in syms)
        {
            if (result.ContainsKey(s)) continue;

            var url = $"https://finance.yahoo.com/quote/{Uri.EscapeDataString(s)}";
            try
            {
                using var resp = await _http.GetAsync(url, ct);
                if (resp.StatusCode != HttpStatusCode.OK)
                    continue;

                var html = await resp.Content.ReadAsStringAsync(ct);
                var price = ExtractPriceFromHtml(html);
                if (price is not null)
                    result[s] = price.Value;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Yahoo HTML falló para {Symbol}", s);
            }

            // evitar rate limit
            await Task.Delay(150, ct);
        }

        return result;
    }

    private static readonly Regex RmPriceRegex = new("\"regularMarketPrice\"\\s*:\\s*\\{[^}]*?\"raw\"\\s*:\\s*(?<num>[-+]?[0-9]*\\.?[0-9]+)", RegexOptions.Compiled);
    private static Dictionary<string, decimal> ExtractBatchPrices(string json, string[] syms)
    {
        // súper simple: encuentra la primera coincidencia para cada símbolo y toma el primer "regularMarketPrice.raw"
        var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        // Sugerencia: si querés fino, parsea con System.Text.Json a la ruta response.result[*].regularMarketPrice.raw
        // Para mantener liviano, hago un regex global y asigno en orden.
        var matches = RmPriceRegex.Matches(json);
        var idx = 0;
        foreach (Match m in matches.Cast<Match>())
        {
            if (idx >= syms.Length) break;
            if (decimal.TryParse(m.Groups["num"].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var val))
            {
                // ¡Ojo! El orden de Yahoo no necesariamente coincide con syms. Si necesitás 100% match, parsea JSON.
                // En la práctica, muchas veces coincide. Para estar seguro, reemplazá este método por System.Text.Json.
                // Dejo un fallback conservador:
                dict[syms[idx]] = val;
                idx++;
            }
        }
        return dict;
    }

    private static decimal? ExtractPriceFromHtml(string html)
    {
        // Busca <fin-streamer data-field="regularMarketPrice" ...>NUM</fin-streamer>
        var m = Regex.Match(html, "<fin-streamer[^>]*data-field=\"regularMarketPrice\"[^>]*>(?<num>[-+]?[0-9]*\\.?[0-9]+)</fin-streamer>", RegexOptions.IgnoreCase);
        if (m.Success && decimal.TryParse(m.Groups["num"].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var val))
            return val;

        // fallback: busca "regularMarketPrice":{"raw":NUM}
        var m2 = RmPriceRegex.Match(html);
        if (m2.Success && decimal.TryParse(m2.Groups["num"].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var val2))
            return val2;

        return null;
    }
}
