using ApiClient;
using DTOs;

namespace Services
{
    public class CryptoService
    {
        private readonly BinanceClient _binance;
        public CryptoService(BinanceClient binance) => _binance = binance;

        public Task<List<QuoteDTO>> GetQuotesAsync(IEnumerable<string> symbols) =>
            _binance.GetSpotPricesAsync(symbols);
    }
}
