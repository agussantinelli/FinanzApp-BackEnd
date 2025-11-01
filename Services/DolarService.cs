using System.Net.Http.Json;
using DTOs;

namespace Services
{
    public class DolarService
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "https://dolarapi.com/v1";

        public DolarService(HttpClient http) => _http = http;

        public async Task<List<DolarDTO>> GetCotizacionesAsync(CancellationToken ct = default)
        {
            var fromApi = await _http.GetFromJsonAsync<List<DolarDTO>>($"{BaseUrl}/dolares", ct);
            return fromApi ?? new();
        }

        public async Task<decimal> GetTcAsync(string preferido = "CCL", CancellationToken ct = default)
        {
            if (preferido.Equals("CCL", StringComparison.OrdinalIgnoreCase))
            {
                var ccl = await _http.GetFromJsonAsync<DolarDTO>($"{BaseUrl}/ambito/dolares/contadoconliqui", ct);
                if (ccl?.Venta is decimal v1 && v1 > 0) return v1;
            }
            else if (preferido.Equals("MEP", StringComparison.OrdinalIgnoreCase))
            {
                var mep = await _http.GetFromJsonAsync<DolarDTO>($"{BaseUrl}/ambito/dolares/bolsa", ct);
                if (mep?.Venta is decimal v2 && v2 > 0) return v2;
            }

            var todos = await GetCotizacionesAsync(ct);
            var elegido = todos.FirstOrDefault(d =>
                preferido.Equals("CCL", StringComparison.OrdinalIgnoreCase)
                    ? EsCCL(d.Nombre)
                    : EsMEP(d.Nombre));

            return elegido?.Venta ?? 0m;
        }

        private static bool EsCCL(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            var n = s.Trim().ToLowerInvariant();
            return n.Contains("contado") || n.Contains("liqui") || n.Contains("liquid") || n.Contains("ccl");
        }

        private static bool EsMEP(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            var n = s.Trim().ToLowerInvariant();
            // "Bolsa/MEP", "MEP", "dólar bolsa"
            return n.Contains("mep") || n.Contains("bolsa");
        }
    }
}
