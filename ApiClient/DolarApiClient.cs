using DTOs;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiClient
{
    public class DolarApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://dolarapi.com/v1/dolares";

        public DolarApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<DolarDTO>> GetCotizacionesAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<List<DolarDTO>>(BaseUrl);
            return result ?? new List<DolarDTO>();
        }
    }
}
