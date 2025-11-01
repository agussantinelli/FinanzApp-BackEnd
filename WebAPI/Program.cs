using ApiClient;
using Services;
using WebAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HTTP y dependencias
builder.Services.AddHttpClient();
builder.Services.AddScoped<ApiClient.BinanceClient>();
builder.Services.AddScoped<ApiClient.YahooFinanceClient>();
builder.Services.AddScoped<Services.CryptoService>();
builder.Services.AddScoped<Services.StocksService>();
builder.Services.AddScoped<Services.CedearsService>();

// CORS - Permitir acceso desde tu frontend
var corsPolicy = "_finanzappCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",   
                "https://localhost:3000"  
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors(corsPolicy);

#region Endpoints
app.MapDolarEndpoints();
app.MapCedearsEndpoints();
app.MapCryptoEndpoints();
app.MapStocksEndpoints();

#endregion

app.Run();
