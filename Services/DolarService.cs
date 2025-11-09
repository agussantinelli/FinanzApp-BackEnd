using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DTOs;
using System.Linq;

public class DolarService
{
    private readonly HttpClient _http;

    public DolarService(HttpClient http)
    {
        _http = http;
        if (!_http.DefaultRequestHeaders.UserAgent.Any())
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("FinanzApp/1.0 (+https://localhost)");
    }

    private sealed class DolarApiItem
    {
        [JsonPropertyName("casa")] public string? Casa { get; set; }
        [JsonPropertyName("nombre")] public string? Nombre { get; set; }
        [JsonPropertyName("compra")] public decimal? Compra { get; set; }
        [JsonPropertyName("venta")] public decimal? Venta { get; set; }
    }

    public async Task<List<DolarDTO>> GetCotizacionesAsync(CancellationToken ct = default)
    {
        var url = "https://dolarapi.com/v1/dolares";
        List<DolarApiItem>? api;

        try
        {
            api = await _http.GetFromJsonAsync<List<DolarApiItem>>(url, cancellationToken: ct);
        }
        catch
        {
            return new List<DolarDTO>();
        }

        if (api is null || api.Count == 0) return new List<DolarDTO>();

        var salida = new List<DolarDTO>();

        void TryAdd(params string[] claves)
        {
            var item = api.FirstOrDefault(x =>
                claves.Any(k =>
                    string.Equals(x.Casa, k, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Nombre, k, StringComparison.OrdinalIgnoreCase)));

            if (item is null || item.Compra is null || item.Venta is null) return;

            var nombreNormalizado =
                (item.Casa?.ToLowerInvariant(), item.Nombre?.ToLowerInvariant()) switch
                {
                    ("bolsa", _) or (_, "bolsa") => "MEP",
                    ("contadoconliqui", _) or (_, "contado con liquidación") or (_, "contado con liquidacion") => "CCL",
                    ("oficial", _) or (_, "oficial") => "Oficial",
                    ("blue", _) or (_, "blue") => "Blue",
                    ("tarjeta", _) or (_, "tarjeta") or (_, "qatar") or (_, "solidario") or (_, "turista") => "Tarjeta",
                    ("mayorista", _) or (_, "mayorista") => "Mayorista",
                    _ => item.Nombre ?? item.Casa ?? "—"
                };

            salida.Add(new DolarDTO
            {
                Nombre = nombreNormalizado,
                Compra = item.Compra ?? 0,
                Venta = item.Venta ?? 0
            });
        }

        // Orden clásico
        TryAdd("oficial", "Oficial");
        TryAdd("blue", "Blue");
        TryAdd("tarjeta", "Tarjeta", "Qatar", "Solidario", "Turista");
        TryAdd("bolsa", "Bolsa"); // MEP
        TryAdd("contadoconliqui", "Contado con Liquidación", "Contado con Liquidacion"); // CCL
        TryAdd("mayorista", "Mayorista");

        // El resto (si viene algo extra)
        foreach (var rest in api)
        {
            if (rest.Compra is null || rest.Venta is null) continue;

            var nombre = rest.Nombre ?? rest.Casa ?? "—";
            var ya = salida.Any(s => s.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
            if (ya) continue;

            salida.Add(new DolarDTO
            {
                Nombre = nombre,
                Compra = rest.Compra ?? 0,
                Venta = rest.Venta ?? 0
            });
        }

        return salida;
    }

    public async Task<(decimal tc, string nombre)> GetTcAsync(string preferido = "CCL", CancellationToken ct = default)
    {
        var lista = await GetCotizacionesAsync(ct);

        // Busco por nombre normalizado exacto
        var elegido = lista.FirstOrDefault(x =>
            string.Equals(x.Nombre, preferido, StringComparison.OrdinalIgnoreCase));

        if (elegido.Nombre is null || elegido.Venta <= 0)
        {
            // si pediste CCL y no hay, probá MEP, y viceversa
            if (preferido.Equals("CCL", StringComparison.OrdinalIgnoreCase))
                elegido = lista.FirstOrDefault(x => x.Nombre.Equals("MEP", StringComparison.OrdinalIgnoreCase));
            else if (preferido.Equals("MEP", StringComparison.OrdinalIgnoreCase))
                elegido = lista.FirstOrDefault(x => x.Nombre.Equals("CCL", StringComparison.OrdinalIgnoreCase));
        }

        // Más fallbacks
        if (elegido.Nombre is null || elegido.Venta <= 0)
            elegido = lista.FirstOrDefault(x => x.Nombre.Equals("Blue", StringComparison.OrdinalIgnoreCase));

        if (elegido.Nombre is null || elegido.Venta <= 0)
            elegido = lista.FirstOrDefault(x => x.Nombre.Equals("Oficial", StringComparison.OrdinalIgnoreCase));

        // Último recurso
        if (elegido.Nombre is null || elegido.Venta <= 0)
            elegido = lista.FirstOrDefault(x => x.Venta > 0);

        // Si no hay nada, devuelvo 0
        if (elegido.Nombre is null || elegido.Venta <= 0)
            return (0m, preferido);

        return (elegido.Venta, elegido.Nombre);
    }
}
