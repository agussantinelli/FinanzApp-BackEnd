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

        public async Task<List<CryptoTopDTO>> GetTopAsync(int limit = 10, CancellationToken ct = default)
        {
            var fetch = Math.Clamp(limit * 4, 28, 100);

            var cacheKey = $"crypto_top_nostable_noderiv_{fetch}";
            if (_cache.TryGetValue(cacheKey, out List<CryptoTopDTO>? cached) && cached is not null)
                return cached.Take(limit).ToList();

            //  CoinGecko → fallback CoinCap
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

            // StablesCoin más comunes
            var stableSyms = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "USDT","USDC","DAI","BUSD","TUSD","USDP","PAX","GUSD","FDUSD","EURS","EURT","USDN","USDD","USDE"
            };

            // Derivados/pegged/wrapped/liquid staking (principalmente de ETH/BTC)
            var derivativeSyms = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "STETH","RETH", "WBT","CBETH","WBTC","WETH","WBETH","AETHC","FRETH","FRXETH","ANKRETH","OSETH","SWETH","RSETH"
            };

            bool IsStable(CryptoTopDTO c)
            {
                var sym = c.Symbol.Trim().ToUpperInvariant();
                var name = c.Name.Trim().ToUpperInvariant();

                if (stableSyms.Contains(sym)) return true;
                if (name.Contains(" STABLE") || name.Contains(" USDT") || name.Contains(" USDC") || name.Contains(" USD "))
                    return true;

                var p = c.PriceUsd;
                if (p >= 0.94m && p <= 1.06m) return true;
                return false;
            }

            bool IsDerivative(CryptoTopDTO c)
            {
                var sym = (c.Symbol ?? "").Trim().ToUpperInvariant();
                var name = (c.Name ?? "").Trim().ToUpperInvariant();

                if (sym is "ETH" or "BTC") return false;

                if (sym is "STETH" or "WETH" or "WBTC" or "CBETH" or "RETH" or "WBETH" or "WBT" or "FRXETH" or "RSETH" or "OSETH" or "ANKRETH")
                    return true;

                if (name.Contains("STETH") ||
                    name.Contains("LIDO") ||
                    name.Contains("STAKED") ||
                    name.Contains("WRAPPED") ||
                    name.Contains("RESTAKED") ||
                    name.Contains("LIQUID STAKED"))
                    return true;

                return false;
            }


            var filtered = raw
                .Where(c => !IsStable(c) && !IsDerivative(c))
                .OrderBy(c => c.Rank)            
                .ThenByDescending(c => c.PriceUsd)
                .DistinctBy(c => c.Symbol.ToUpperInvariant())
                .Take(limit)
                .ToList();

            _cache.Set(cacheKey, filtered, TimeSpan.FromMinutes(2));
            return filtered;
        }



    }
}
