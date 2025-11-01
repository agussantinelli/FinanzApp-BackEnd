using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace WebAPI.Endpoints
{
    public static class CedearsEndpoints
    {
        public static void MapCedearsEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/cedears");
            group.MapGet("/duals", async (
                CedearsService svc,
                [FromQuery] string dolar = "CCL"
            ) =>
            {
                var data = await svc.GetCedearDualsAsync(dolar);
                return Results.Ok(data);
            });
        }
    }
}
