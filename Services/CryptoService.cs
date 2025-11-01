using ApiClient;
using DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace Services
{
    public class CryptoService
    {
        private readonly BinanceClient _binance;
        private readonly CoinGeckoClient _gecko;
        private readonly IMemoryCache _cache;
        private readonly CoinCapClient _coincap;

        public CryptoService(BinanceClient binance, CoinGeckoClient gecko, IMemoryCache cache)
        {
            _binance = binance; _gecko = gecko; _cache = cache;
        }

        public Task<List<QuoteDTO>> GetQuotesAsync(IEnumerable<string> symbols, CancellationToken ct = default)
            => _binance.GetSpotPricesAsync(symbols, ct);

        public async Task<List<CryptoTopDTO>> GetTopAsync(int limit = 6, CancellationToken ct = default)
        {
            var key = $"crypto_top_{limit}";
            if (_cache.TryGetValue(key, out List<CryptoTopDTO>? cached) && cached is not null)
                return cached;

            List<CryptoTopDTO> data;
            try
            {
                data = await _gecko.GetTopAsync(limit, ct);
                if (data.Count == 0) throw new Exception("Gecko vacío");
            }
            catch
            {
                data = await _coincap.GetTopAsync(limit, ct); 
            }

            _cache.Set(key, data, TimeSpan.FromMinutes(2));
            return data;
        }

    }
}
