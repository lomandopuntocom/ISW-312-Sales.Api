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

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        var itemsToUpdate = await db.CommandItems
            .Where(x => x.Status == "PENDIENTE" || x.Status == "EN_PREPARACION" || x.Status == "LISTO" || x.Status == "SENT")
            .ToListAsync();

        var commandsToUpdate = await db.Commands
            .Where(x => x.Status == "PENDIENTE" || x.Status == "EN_PREPARACION" || x.Status == "LISTO" || x.Status == "SENT")
            .ToListAsync();

        if (itemsToUpdate.Any() || commandsToUpdate.Any())
        {
            Console.WriteLine($"Migrating {itemsToUpdate.Count} command items and {commandsToUpdate.Count} commands with legacy statuses...");
            foreach (var item in itemsToUpdate)
            {
                if (item.Status == "PENDIENTE" || item.Status == "SENT") item.Status = "PENDING";
                else if (item.Status == "EN_PREPARACION") item.Status = "IN_PROGRESS";
                else if (item.Status == "LISTO") item.Status = "READY";
            }
            foreach (var cmd in commandsToUpdate)
            {
                if (cmd.Status == "PENDIENTE" || cmd.Status == "SENT") cmd.Status = "PENDING";
                else if (cmd.Status == "EN_PREPARACION") cmd.Status = "IN_PROGRESS";
                else if (cmd.Status == "LISTO") cmd.Status = "READY";
            }
            await db.SaveChangesAsync();
            Console.WriteLine("Migration completed successfully.");
        }

        // Delete ticket 6061 as requested
        var targetTicketIdEndsWith = "6061";
        var commandsToDelete = await db.Commands
            .Where(x => x.TicketCen.ToString().EndsWith(targetTicketIdEndsWith))
            .ToListAsync();

        if (commandsToDelete.Any())
        {
            var commandIds = commandsToDelete.Select(c => c.Id).ToList();
            var itemsToDelete = await db.CommandItems
                .Where(x => commandIds.Contains(x.CommandId))
                .ToListAsync();

            db.CommandItems.RemoveRange(itemsToDelete);
            db.Commands.RemoveRange(commandsToDelete);
            await db.SaveChangesAsync();
            Console.WriteLine($"Successfully deleted {commandsToDelete.Count} commands and {itemsToDelete.Count} command items for ticket ending with {targetTicketIdEndsWith}.");
        }

        // Diagnostic logging of all remaining commands
        var allCommands = await db.Commands.Include(x => x.Items).ToListAsync();
        Console.WriteLine($"Total active commands in DB: {allCommands.Count}");
        foreach (var cmd in allCommands)
        {
            Console.WriteLine($"- Command {cmd.CommandNumber} (Status: {cmd.Status}, TicketCen: {cmd.TicketCen}):");
            foreach (var item in cmd.Items)
            {
                Console.WriteLine($"   * Item ProductCen: {item.ProductCen}, Status: {item.Status}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error running startup KDS migration: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // 👈 Agregar
}

app.UseCors("AllowFrontend");
app.MapControllers();
app.Run();