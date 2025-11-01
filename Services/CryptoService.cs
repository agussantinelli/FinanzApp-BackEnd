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
            var fetch = Math.Clamp(limit * 3, 10, 60);

            var key = $"crypto_top_nostable_{fetch}";
            if (_cache.TryGetValue(key, out List<CryptoTopDTO>? cached) && cached is not null)
                return cached.Take(limit).ToList();

            List<CryptoTopDTO> raw;
            try
            {
                raw = await _gecko.GetTopAsync(fetch, ct);
                if (raw.Count == 0) throw new Exception("Gecko vacío");
            }
            catch
            {
                raw = await _coincap.GetTopAsync(fetch, ct);
            }

            var stableSyms = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "USDT","USDC","DAI","BUSD","TUSD","USDP","PAX","UST","USTC","GUSD","FDUSD","EURS","EURT",
                "USD","USDN","USDD"
            };

            bool IsLikelyStable(CryptoTopDTO c)
            {
                var sym = c.Symbol.Trim().ToUpperInvariant();
                var name = c.Name.Trim().ToUpperInvariant();

                if (stableSyms.Contains(sym)) return true;
                if (name.Contains("STABLE") || name.Contains("USD ") || name.EndsWith(" USD") || name.Contains("USDT") || name.Contains("USDC"))
                    return true;

                var p = c.PriceUsd;
                if (p >= 0.94m && p <= 1.06m) return true;

                return false;
            }

            var filtered = raw.Where(c => !IsLikelyStable(c))
                              .OrderBy(c => c.Rank)       // respetar market cap rank
                              .ThenByDescending(c => c.PriceUsd)
                              .Take(limit)
                              .ToList();

            _cache.Set(key, filtered, TimeSpan.FromMinutes(2));
            return filtered;
        }


    }
}
