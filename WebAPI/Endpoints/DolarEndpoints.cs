using DTOs;

public static class DolarEndpoints
{
    public static IEndpointRouteBuilder MapDolarEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dolar/cotizaciones", async (DolarService svc, CancellationToken ct) =>
        {
            var data = await svc.GetCotizacionesAsync(ct);
            return Results.Ok(data);
        })
        .WithName("GetDolarCotizaciones")
        .WithTags("Dólar");

        return app;
    }
}
