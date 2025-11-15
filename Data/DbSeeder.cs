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
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("FinanzAppSeeder/1.0 (+https://github.com/agussantinelli)");
        http.DefaultRequestHeaders.Accept.ParseAdd("application/json");

        await SeedPaisesAsync(context, http, ct);
        await SeedProvinciasAsync(context, http, ct);
        await SeedLocalidadesAsync(context, http, ct);
    }

    // PAISES (RESTCOUNTRIES, NOMBRE EN ESPAÑOL) 
    private static async Task SeedPaisesAsync(DBFinanzasContext context, HttpClient http, CancellationToken ct)
    {
        if (await context.Paises.AnyAsync(ct))
            return;

        const string url = "https://restcountries.com/v3.1/all?fields=name,translations,cca2,cca3";

        HttpResponseMessage resp;
        try
        {
            resp = await http.GetAsync(url, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Seeder] Error llamando a RestCountries: {ex.Message}");
            return;
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            Console.WriteLine($"[Seeder] RestCountries devolvió {(int)resp.StatusCode} ({resp.StatusCode}). Cuerpo: {body}");
            return;
        }

        var json = await resp.Content.ReadAsStringAsync(ct);
        var countries = JsonSerializer.Deserialize<List<RestCountryDto>>(json, JsonOptions) ?? new();

        var list = new List<Pais>();

        foreach (var c in countries)
        {
            if (string.IsNullOrWhiteSpace(c.Cca2) || string.IsNullOrWhiteSpace(c.Cca3))
                continue;

            var iso2 = c.Cca2.ToUpperInvariant();
            var iso3 = c.Cca3.ToUpperInvariant();

            var nameEs = c.Translations?.Spa?.Common?.Trim();
            var nameEn = c.Name?.Common?.Trim();

            var name = !string.IsNullOrWhiteSpace(nameEs)
                ? nameEs
                : !string.IsNullOrWhiteSpace(nameEn)
                    ? nameEn
                    : iso3;

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
        {
            Console.WriteLine("[Seeder] No se pudo construir la lista de países a partir de RestCountries.");
            return;
        }

        await context.Paises.AddRangeAsync(list, ct);
        await context.SaveChangesAsync(ct);
        Console.WriteLine($"[Seeder] Seed de Paises completado. Total: {list.Count}");
    }

    //  PROVINCIAS ARGENTINAS (GEOREF) 
    private static async Task SeedProvinciasAsync(DBFinanzasContext context, HttpClient http, CancellationToken ct)
    {
        if (await context.Provincias.AnyAsync(ct))
            return;

        var argentina = await context.Paises
            .FirstOrDefaultAsync(p => p.CodigoIso2 == "AR", ct);

        if (argentina is null)
        {
            Console.WriteLine("[Seeder] No se encontró país 'AR' para seedear provincias.");
            return;
        }

        const string url =
            "https://apis.datos.gob.ar/georef/api/provincias?max=24&campos=id,nombre";

        HttpResponseMessage resp;
        try
        {
            resp = await http.GetAsync(url, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Seeder] Error llamando a Georef provincias: {ex.Message}");
            return;
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            Console.WriteLine($"[Seeder] Georef provincias devolvió {(int)resp.StatusCode} ({resp.StatusCode}). Cuerpo: {body}");
            return;
        }

        var json = await resp.Content.ReadAsStringAsync(ct);
        var data = JsonSerializer.Deserialize<GeorefProvinciaResponse>(json, JsonOptions);

        if (data?.Provincias is null || data.Provincias.Count == 0)
        {
            Console.WriteLine("[Seeder] Georef provincias no devolvió datos.");
            return;
        }

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
        Console.WriteLine($"[Seeder] Seed de Provincias completado. Total: {provincias.Count}");
    }

    // LOCALIDADES ARGENTINAS (GEOREF) 
    private static async Task SeedLocalidadesAsync(DBFinanzasContext context, HttpClient http, CancellationToken ct)
    {
        if (await context.Localidades.AnyAsync(ct))
            return;

        var provincias = await context.Provincias.ToListAsync(ct);
        if (provincias.Count == 0)
        {
            Console.WriteLine("[Seeder] No hay provincias en la BD; no se pueden seedear localidades.");
            return;
        }

        var provinciasByNombre = provincias
            .GroupBy(p => p.Nombre.ToUpperInvariant())
            .ToDictionary(g => g.Key, g => g.First());

        const string url =
            "https://apis.datos.gob.ar/georef/api/localidades?max=5000&aplanar=true&campos=id,nombre,provincia.id,provincia.nombre";

        HttpResponseMessage resp;
        try
        {
            resp = await http.GetAsync(url, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Seeder] Error llamando a Georef localidades: {ex.Message}");
            return;
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            Console.WriteLine($"[Seeder] Georef localidades devolvió {(int)resp.StatusCode} ({resp.StatusCode}). Cuerpo: {body}");
            return;
        }

        var json = await resp.Content.ReadAsStringAsync(ct);
        var data = JsonSerializer.Deserialize<GeorefLocalidadResponse>(json, JsonOptions);

        if (data?.Localidades is null || data.Localidades.Count == 0)
        {
            Console.WriteLine("[Seeder] Georef localidades no devolvió datos.");
            return;
        }

        var localidades = new List<Localidad>();

        foreach (var loc in data.Localidades)
        {
            if (string.IsNullOrWhiteSpace(loc.Nombre) ||
                string.IsNullOrWhiteSpace(loc.ProvinciaNombre))
                continue;

            var provKey = loc.ProvinciaNombre.ToUpperInvariant();

            if (!provinciasByNombre.TryGetValue(provKey, out var provincia))
                continue;

            localidades.Add(new Localidad
            {
                Nombre = loc.Nombre.Trim(),
                ProvinciaId = provincia.Id
            });
        }

        var agrupadas = localidades
            .GroupBy(l => new { l.Nombre, l.ProvinciaId })
            .Select(g => g.First())
            .ToList();

        await context.Localidades.AddRangeAsync(agrupadas, ct);
        await context.SaveChangesAsync(ct);
        Console.WriteLine($"[Seeder] Seed de Localidades completado. Total: {agrupadas.Count}");
    }

    // DTOs internos 
    private sealed class RestCountryDto
    {
        public NameDto? Name { get; set; }
        public string Cca2 { get; set; } = string.Empty;
        public string Cca3 { get; set; } = string.Empty;
        public RestCountryTranslationsDto? Translations { get; set; }
    }

    private sealed class NameDto
    {
        public string Common { get; set; } = string.Empty;
    }

    private sealed class RestCountryTranslationsDto
    {
        public RestCountrySpaDto? Spa { get; set; }
    }

    private sealed class RestCountrySpaDto
    {
        public string Common { get; set; } = string.Empty;
        public string Official { get; set; } = string.Empty;
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
