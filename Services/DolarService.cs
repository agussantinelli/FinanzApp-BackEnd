using ApiClient;
using DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
    public class DolarService
    {
        private readonly DolarApiClient _apiClient;

        public DolarService(DolarApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<DolarDTO>> GetCotizacionesAsync()
        {
            var cotizaciones = await _apiClient.GetCotizacionesAsync();
            return cotizaciones;
        }
    }
}
