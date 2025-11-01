using ApiClient;
using DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace Services
{
    public class DolarService
    {
        private readonly DolarApiClient _api;
        private readonly IMemoryCache _cache;

        private static readonly string CACHE_KEY = "dolar_quotes";

        public DolarService(DolarApiClient api, IMemoryCache cache)
        {
            _api = api; _cache = cache;
        }

        public async Task<IEnumerable<DolarDTO>> GetCotizacionesAsync()
        {
            if (_cache.TryGetValue(CACHE_KEY, out List<DolarDTO>? cached) && cached is not null)
                return cached;

            var data = await _api.GetCotizacionesAsync();
            // cachea 2 minutos
            _cache.Set(CACHE_KEY, data, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            });

            return data;
        }
    }
}
