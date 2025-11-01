using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Services;

namespace WebAPI.Endpoints
{
    public static class DolarEndpoints
    {
        public static void MapDolarEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/dolar");

            group.MapGet("/cotizaciones", async (DolarService service) =>
            {
                var cotizaciones = await service.GetCotizacionesAsync();
                return Results.Ok(cotizaciones);
            });
        }
    }
}
