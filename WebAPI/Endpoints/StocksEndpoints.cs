// WebAPI/Endpoints/StocksEndpoints.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Services;
using DTOs;

namespace WebAPI.Endpoints;

public static class StocksEndpoints
{
    public sealed record DualPairReq(string localBA, string usa, decimal? cedearRatio);

    public static IEndpointRouteBuilder MapStocksEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/stocks/duals", async (
            StocksService stocks,
            HttpContext ctx,
            string? dolar,                            
            List<DualPairReq> pairs,
            CancellationToken ct) =>
        {
            var prefer = string.IsNullOrWhiteSpace(dolar) ? "CCL" : dolar!;
            var arr = pairs.Select(p => (p.localBA, p.usa, p.cedearRatio)).ToArray();
            var data = await stocks.GetDualsAsync(arr, prefer, ct);
            return Results.Ok(data);
        });

        return app;
    }
}
