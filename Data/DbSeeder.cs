using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Data;

public static class DbSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task SeedAsync(DBFinanzasContext context, CancellationToken ct = default)
    {
        // HttpClient local para las llamadas de seed
        using var http = new HttpClient();

        await SeedPaisesAsync(context, http, ct);
        await SeedProvinciasAsync(context, http, ct);
        await SeedLocalidadesAsync(context, http, ct);
    }

    //  PAISES (RESTCOUNTRIES)
    private static async Task SeedPaisesAsync(DBFinanzasContext context, HttpClient http, CancellationToken ct)
    {
        if (await context.Paises.AnyAsync(ct))
            return; // ya hay datos, no hacemos nada

        const string url = "https://restcountries.com/v3.1/all";

        var resp = await http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        var countries = JsonSerializer.Deserialize<List<RestCountryDto>>(json, JsonOptions) ?? new();

        var list = new List<Pais>();

        foreach (var c in countries)
        {
            if (string.IsNullOrWhiteSpace(c.Cca2) || string.IsNullOrWhiteSpace(c.Cca3))
                continue;

            var iso2 = c.Cca2.ToUpperInvariant();
            var iso3 = c.Cca3.ToUpperInvariant();
            var name = c.Name?.Common?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                name = iso3;

            // evitamos duplicados por ISO2
            if (list.Any(p => p.CodigoIso2 == iso2))
                continue;

            list.Add(new Pais
            {
                Nombre = name,
                CodigoIso2 = iso2,
                CodigoIso3 = iso3,
                EsArgentina = string.Equals(iso2, "AR", StringComparison.OrdinalIgnoreCase)
            });
        }

        if (list.Count == 0)
            return;

        await context.Paises.AddRangeAsync(list, ct);
        await context.SaveChangesAsync(ct);
    }

    //  PROVINCIAS ARGENTINAS (GEOREF)
    private static async Task SeedProvinciasAsync(DBFinanzasContext context, HttpClient http, CancellationToken ct)
    {
        if (await context.Provincias.AnyAsync(ct))
            return; // ya están cargadas

        var argentina = await context.Paises
            .FirstOrDefaultAsync(p => p.CodigoIso2 == "AR", ct);

        if (argentina is null)
            return; // por las dudas, si falló el seeding de países

        const string url =
            "https://apis.datos.gob.ar/georef/api/provincias?max=24&campos=id,nombre";

        var resp = await http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        var data = JsonSerializer.Deserialize<GeorefProvinciaResponse>(json, JsonOptions);

        if (data?.Provincias is null || data.Provincias.Count == 0)
            return;

        var provincias = data.Provincias
            .Where(p => !string.IsNullOrWhiteSpace(p.Nombre))
            .Select(p => new Provincia
            {
                Nombre = p.Nombre.Trim(),
                PaisId = argentina.Id
            })
            .ToList();

        await context.Provincias.AddRangeAsync(provincias, ct);
        await context.SaveChangesAsync(ct);
    }

    //  LOCALIDADES ARGENTINAS (GEOREF)
    private static async Task SeedLocalidadesAsync(DBFinanzasContext context, HttpClient http, CancellationToken ct)
    {
        if (await context.Localidades.AnyAsync(ct))
            return; // ya hay localidades

        // necesitamos las provincias que ya se seedearon
        var provincias = await context.Provincias.ToListAsync(ct);
        if (provincias.Count == 0)
            return;

        var provinciasByNombre = provincias
            .GroupBy(p => p.Nombre.ToUpperInvariant())
            .ToDictionary(g => g.Key, g => g.First());

        const string url =
            "https://apis.datos.gob.ar/georef/api/localidades?max=5000&aplanar=true&campos=id,nombre,provincia.id,provincia.nombre";

        var resp = await http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        var data = JsonSerializer.Deserialize<GeorefLocalidadResponse>(json, JsonOptions);

        if (data?.Localidades is null || data.Localidades.Count == 0)
            return;

        var localidades = new List<Localidad>();

        foreach (var loc in data.Localidades)
        {
            if (string.IsNullOrWhiteSpace(loc.Nombre) ||
                string.IsNullOrWhiteSpace(loc.ProvinciaNombre))
                continue;

            var provKey = loc.ProvinciaNombre.ToUpperInvariant();

            if (!provinciasByNombre.TryGetValue(provKey, out var provincia))
                continue; // por si algún nombre no matchea

            localidades.Add(new Localidad
            {
                Nombre = loc.Nombre.Trim(),
                ProvinciaId = provincia.Id
            });
        }

        // Evitar duplicados por (Nombre, ProvinciaId)
        var agrupadas = localidades
            .GroupBy(l => new { l.Nombre, l.ProvinciaId })
            .Select(g => g.First())
            .ToList();

        await context.Localidades.AddRangeAsync(agrupadas, ct);
        await context.SaveChangesAsync(ct);
    }

    //  DTOs internos para deserializar APIs

    private sealed class RestCountryDto
    {
        public NameDto? Name { get; set; }
        public string Cca2 { get; set; } = string.Empty;
        public string Cca3 { get; set; } = string.Empty;
    }

    private sealed class NameDto
    {
        public string Common { get; set; } = string.Empty;
    }

    private sealed class GeorefProvinciaResponse
    {
        [JsonPropertyName("provincias")]
        public List<GeorefProvinciaDto> Provincias { get; set; } = new();
    }

    private sealed class GeorefProvinciaDto
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    private sealed class GeorefLocalidadResponse
    {
        [JsonPropertyName("localidades")]
        public List<GeorefLocalidadDto> Localidades { get; set; } = new();
    }

    private sealed class GeorefLocalidadDto
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("provincia_id")]
        public string ProvinciaId { get; set; } = string.Empty;

        [JsonPropertyName("provincia_nombre")]
        public string ProvinciaNombre { get; set; } = string.Empty;
    }
}
