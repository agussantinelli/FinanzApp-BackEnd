using Microsoft.AspNetCore.Builder;
using Services;

namespace WebAPI.Endpoints
{
    public static class StocksEndpoints
    {
        public static void MapStocksEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/stocks");

            group.MapPost("/duals", async (List<(string localBA, string usa)> pairs, string? dolar = "CCL", StocksService svc) =>
            {
                var data = await svc.GetDualsAsync(pairs.ToArray(), dolar ?? "CCL");
                return Results.Ok(data);
            });
        }
    }
}
