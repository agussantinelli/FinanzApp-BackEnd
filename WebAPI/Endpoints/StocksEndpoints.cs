using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI.Endpoints
{
    public static class StocksEndpoints
    {
        public static void MapStocksEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/stocks");

            group.MapPost("/duals", async (
                [FromBody] List<(string localBA, string usa)> pairs,
                StocksService svc,
                [FromQuery] string dolar = "CCL"
            ) =>
            {
                var data = await svc.GetDualsAsync(pairs.ToArray(), dolar);
                return Results.Ok(data);
            });
        }
    }
}
