using ApiClient;
using Services;
using WebAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

// HTTP y dependencias
builder.Services.AddHttpClient();
builder.Services.AddScoped<DolarApiClient>();
builder.Services.AddScoped<DolarService>();
builder.Services.AddScoped<BinanceClient>();
builder.Services.AddScoped<CoinGeckoClient>();
builder.Services.AddScoped<YahooFinanceClient>();
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
