using System.Net;
using System.Text.Json;
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
        _http.DefaultRequestHeaders.Accept.ParseAdd("application/json,text/plain,*/*");
        _http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("es-AR,es;q=0.9,en-US;q=0.8,en;q=0.7");
        _http.DefaultRequestHeaders.Referrer = new Uri("https://finance.yahoo.com/");
    }

    public async Task<Dictionary<string, decimal>> GetQuotesAsync(IEnumerable<string> symbols, CancellationToken ct = default)
    {
        var syms = symbols
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        // 1) Batch (puede devolver 401/403)
        var joined = string.Join(",", syms.Select(Uri.EscapeDataString));
        var batchEndpoints = new[]
        {
            $"https://query2.finance.yahoo.com/v7/finance/quote?symbols={joined}",
            $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={joined}",
        };

        foreach (var url in batchEndpoints)
        {
            try
            {
                using var resp = await _http.GetAsync(url, ct);
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    var json = await resp.Content.ReadAsStringAsync(ct);
                    var prices = ExtractBatchPricesBySymbol(json); // ✅ mapeo por symbol
                    foreach (var kv in prices)
                        result[kv.Key] = kv.Value;
                    break; // ya obtuve un batch OK; sigo con faltantes
                }
                _log.LogWarning("Yahoo batch {Url} devolvió {Code}", url, (int)resp.StatusCode);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Yahoo batch falló: {Url}", url);
            }
        }

        // 2) Faltantes → fallback por símbolo (Chart API v8 → HTML ?p=)
        var missing = syms.Where(s => !result.ContainsKey(s)).ToArray();
        if (missing.Length > 0)
            _log.LogInformation("Yahoo batch no trajo {Count} símbolos; voy a fallbacks: {Missing}",
                missing.Length, string.Join(", ", missing));

        foreach (var s in missing)
        {
            // 2.a) Chart API v8 (suele funcionar aún con bloqueos del quote)
            var chartUrls = new[]
            {
                $"https://query2.finance.yahoo.com/v8/finance/chart/{Uri.EscapeDataString(s)}?range=1d&interval=1d",
                $"https://query1.finance.yahoo.com/v8/finance/chart/{Uri.EscapeDataString(s)}?range=1d&interval=1d",
            };

            bool done = false;
            foreach (var cu in chartUrls)
            {
                try
                {
                    using var resp = await _http.GetAsync(cu, ct);
                    if (resp.StatusCode != HttpStatusCode.OK) continue;

                    var json = await resp.Content.ReadAsStringAsync(ct);
                    var p = ExtractPriceFromChartJson(json);
                    if (p is decimal pv)
                    {
                        result[s] = pv;
                        done = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Yahoo chart v8 falló para {Symbol} ({Url})", s, cu);
                }
            }
            if (done) { await Task.Delay(120, ct); continue; }

            // 2.b) HTML con ?p=SYMBOL (evita muchos 404/consent)
            var htmlUrl = $"https://finance.yahoo.com/quote/{Uri.EscapeDataString(s)}?p={Uri.EscapeDataString(s)}";
            try
            {
                using var resp = await _http.GetAsync(htmlUrl, ct);
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    var html = await resp.Content.ReadAsStringAsync(ct);
                    var price = ExtractPriceFromHtml(html);
                    if (price is not null)
                    {
                        result[s] = price.Value;
                        done = true;
                    }
                }
                else
                {
                    _log.LogWarning("Yahoo HTML devolvió {Code} para {Symbol}", (int)resp.StatusCode, s);
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Yahoo HTML falló para {Symbol}", s);
            }

            await Task.Delay(120, ct); // evitar rate limit
        }

        return result;
    }

    // ---------- Helpers ----------

    private static Dictionary<string, decimal> ExtractBatchPricesBySymbol(string json)
    {
        var dict = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("quoteResponse", out var qr)) return dict;
            if (!qr.TryGetProperty("result", out var result)) return dict;

            foreach (var item in result.EnumerateArray())
            {
                if (!item.TryGetProperty("symbol", out var symEl)) continue;
                var sym = symEl.GetString();
                if (string.IsNullOrWhiteSpace(sym)) continue;

                decimal? price = null;

                if (item.TryGetProperty("regularMarketPrice", out var rmp))
                {
                    // Puede venir como objeto { raw, fmt } o número directo
                    if (rmp.ValueKind == JsonValueKind.Object && rmp.TryGetProperty("raw", out var raw))
                    {
                        if (raw.ValueKind == JsonValueKind.Number)
                            price = (decimal)raw.GetDouble();
                        else if (raw.ValueKind == JsonValueKind.String &&
                                 decimal.TryParse(raw.GetString(),
                                     System.Globalization.NumberStyles.Any,
                                     System.Globalization.CultureInfo.InvariantCulture,
                                     out var valStr))
                            price = valStr;
                    }
                    else if (rmp.ValueKind == JsonValueKind.Number)
                    {
                        price = (decimal)rmp.GetDouble();
                    }
                }

                if (price is decimal p)
                    dict[sym] = p;
            }
        }
        catch
        {
            // Si cambia el JSON, devolvemos vacío y fallbacks completan.
        }

        return dict;
    }

    // Chart v8 → chart.result[0].meta.regularMarketPrice
    private static decimal? ExtractPriceFromChartJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("chart", out var chart)) return null;
            if (!chart.TryGetProperty("result", out var resArray)) return null;
            var first = resArray.EnumerateArray().FirstOrDefault();
            if (first.ValueKind == JsonValueKind.Undefined) return null;
            if (!first.TryGetProperty("meta", out var meta)) return null;

            // regularMarketPrice puede ser number o string
            if (meta.TryGetProperty("regularMarketPrice", out var rmp))
            {
                if (rmp.ValueKind == JsonValueKind.Number)
                    return (decimal)rmp.GetDouble();

                if (rmp.ValueKind == JsonValueKind.String &&
                    decimal.TryParse(rmp.GetString(),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var valStr))
                    return valStr;
            }
        }
        catch
        {
            // ignorar, uso el siguiente fallback
        }
        return null;
    }

    private static decimal? ExtractPriceFromHtml(string html)
    {
        var m = Regex.Match(
            html,
            "<fin-streamer[^>]*data-field=\"regularMarketPrice\"[^>]*>(?<num>[-+]?[0-9]*\\.?[0-9]+)</fin-streamer>",
            RegexOptions.IgnoreCase);
        if (m.Success && decimal.TryParse(m.Groups["num"].Value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var val1))
            return val1;

        var m2 = RmPriceRegex.Match(html);
        if (m2.Success && decimal.TryParse(m2.Groups["num"].Value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var val2))
            return val2;

        return null;
    }

    private static readonly Regex RmPriceRegex =
        new("\"regularMarketPrice\"\\s*:\\s*\\{[^}]*?\"raw\"\\s*:\\s*(?<num>[-+]?[0-9]*\\.?[0-9]+)",
            RegexOptions.Compiled);
}
