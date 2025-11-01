using Microsoft.AspNetCore.Builder;
using Services;

namespace WebAPI.Endpoints
{
    public static class CryptoEndpoints
    {
        public static void MapCryptoEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/crypto");

            // Precios spot (Binance)
            group.MapGet("/quotes", async (string symbols, CryptoService svc) =>
            {
                var list = symbols.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var data = await svc.GetQuotesAsync(list);
                return Results.Ok(data);
            });

            // Top N por MarketCap (CoinGecko)
            group.MapGet("/top", async (int? limit, CryptoService svc) =>
            {
                var data = await svc.GetTopAsync(limit ?? 10);
                return Results.Ok(data);
            });
        }
    }
}