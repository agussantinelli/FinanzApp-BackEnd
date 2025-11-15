using System.Net;
using System.Text;
using ApiClient;
using Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Services;
using WebAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddMemoryCache();


builder.Services.AddDbContext<DBFinanzasContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FinanzAppDb")));


// DólarAPI (para DolarApiClient)
builder.Services.AddHttpClient<DolarApiClient>(c =>
{
    c.BaseAddress = new Uri("https://dolarapi.com/v1/");
    c.Timeout = TimeSpan.FromSeconds(6);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("FinanzApp/1.0");
    c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

// Servicio de Dólar (agregador)
builder.Services.AddHttpClient<DolarService>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(6);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("FinanzApp/1.0");
    c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

builder.Services.AddHttpClient<CoinGeckoClient>(c =>
{
    c.DefaultRequestHeaders.UserAgent.ParseAdd("FinanzApp/1.0 (+https://localhost)");
    c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

builder.Services.AddHttpClient<CoinCapClient>(c =>
{
    c.DefaultRequestHeaders.UserAgent.ParseAdd("FinanzApp/1.0 (+https://localhost)");
    c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

builder.Services.AddHttpClient<BinanceClient>(c =>
{
    c.DefaultRequestHeaders.UserAgent.ParseAdd("FinanzApp/1.0 (+https://localhost)");
    c.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

builder.Services.AddHttpClient<YahooFinanceClient>(c =>
{
    c.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    c.DefaultRequestHeaders.Accept.ParseAdd("application/json,text/plain,*/*");
    c.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9,es-AR;q=0.8");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip |
                                 DecompressionMethods.Deflate |
                                 DecompressionMethods.Brotli,
        AllowAutoRedirect = true,
        UseCookies = true,
        CookieContainer = new CookieContainer()
    };
});

// Services de dominio

builder.Services.AddScoped<CryptoService>();
builder.Services.AddScoped<StocksService>();
builder.Services.AddScoped<CedearsService>();

// Auth JWT

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("Jwt:Key no está configurado en appsettings.json.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

var corsPolicy = "_finanzappCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DBFinanzasContext>();

    db.Database.Migrate();

    try
    {
        DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Seeder] Error general en seeding: {ex.Message}");
    }
}



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(corsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapDolarEndpoints();
app.MapCedearsEndpoints();
app.MapCryptoEndpoints();
app.MapStocksEndpoints();

app.Run();
