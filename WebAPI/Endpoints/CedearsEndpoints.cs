using Microsoft.AspNetCore.Builder;
using Services;

namespace WebAPI.Endpoints
{
    public static class CedearsEndpoints
    {
        public static void MapCedearsEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/cedears");
            group.MapGet("/duals", async (string? dolar, CedearsService svc) =>
            {
                var data = await svc.GetCedearDualsAsync(dolar ?? "CCL");
                return Results.Ok(data);
            });
        }
    }
}
