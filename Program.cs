using Sales.Api.Application.Abstractions;
using Sales.Api.Infrastructure.Inventory;
using Sales.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore; // 👈 Agregar
using DotNetEnv;

// Load .env from root directory
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
DotNetEnv.Env.Load(Path.GetFullPath(envPath));

var builder = WebApplication.CreateBuilder(args);

var dbHost = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
var dbName = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? throw new InvalidOperationException("DATABASE_NAME not configured");
var dbUser = Environment.GetEnvironmentVariable("DATABASE_USER") ?? throw new InvalidOperationException("DATABASE_USER not configured");
var dbPassword = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? throw new InvalidOperationException("DATABASE_PASSWORD not configured");

var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

var inventoryBaseUrl = Environment.GetEnvironmentVariable("INVENTORY_API_URL")
    ?? "http://localhost:5143";

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "sales")));
builder.Services.AddHttpClient<IInventoryCatalogClient, InventoryCatalogClient>(client =>
{
    client.BaseAddress = new Uri(inventoryBaseUrl);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var corsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS")?.Split(";") 
            ?? ["http://localhost:5173"];

        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // 👈 Agregar
}

app.UseCors("AllowFrontend");
app.MapControllers();
app.Run();