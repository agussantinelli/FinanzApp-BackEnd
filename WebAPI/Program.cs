using System.Net;
using ApiClient;
using Services;
using WebAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cache
builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<DolarApiClient>();

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
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
        AllowAutoRedirect = true,
        UseCookies = true,
        CookieContainer = new CookieContainer()
    };
});

builder.Services.AddHttpClient<DolarService>();

builder.Services.AddScoped<CryptoService>();
builder.Services.AddScoped<StocksService>();
builder.Services.AddScoped<CedearsService>();

// CORS
var corsPolicy = "_finanzappCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(corsPolicy);

// Endpoints
app.MapDolarEndpoints();
app.MapCedearsEndpoints();
app.MapCryptoEndpoints();
app.MapStocksEndpoints();

app.Run();
