using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Services;

namespace WebAPI.Endpoints;

public static class CedearsEndpoints
{
    public static IEndpointRouteBuilder MapCedearsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/cedears/quotes", async (
            CedearsService svc,
            string? dolar,
            List<CedearsService.CedearReq> body,
            CancellationToken ct) =>
        {
            var prefer = string.IsNullOrWhiteSpace(dolar) ? "CCL" : dolar!;
            var data = await svc.GetCedearQuotesAsync(body, prefer, ct);
            return Results.Ok(data);
        });

        app.MapGet("/api/cedears/duals", async (
            CedearsService svc,
            string? dolar,
            CancellationToken ct) =>
        {
            var prefer = string.IsNullOrWhiteSpace(dolar) ? "CCL" : dolar!;
            var reqs = CedearsDefaults.Pairs; 
            var data = await svc.GetCedearQuotesAsync(reqs, prefer, ct);
            return Results.Ok(data);
        });

        app.MapGet("/api/cedears/ratios", () => Results.Ok(CedearsRatios.All));

        return app;
    }
}
