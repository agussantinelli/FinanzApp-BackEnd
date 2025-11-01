using ApiClient;
using Services;
using WebAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HTTP y dependencias
builder.Services.AddHttpClient();
builder.Services.AddScoped<DolarApiClient>();
builder.Services.AddScoped<DolarService>();

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
#endregion

app.Run();
