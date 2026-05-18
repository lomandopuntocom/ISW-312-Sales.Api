using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sales.Api.Infrastructure.Persistence;

namespace Sales.Api.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/dashboard")]
public sealed class DashboardController(SalesDbContext db) : SalesControllerBase(db)
{
    [HttpGet("daily-sales")]
    public async Task<IActionResult> DailySales(string companyCen)
    {
        var company = await FindOrCreateCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var total = await Db.Payments
            .Where(x => Db.Tickets.Any(t => t.Id == x.TicketId && t.CompanyCen == company.Cen))
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        return Ok(new { total, date = DateOnly.FromDateTime(DateTime.UtcNow) });
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> TopProducts(string companyCen)
    {
        var company = await FindOrCreateCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var items = await Db.TicketItems
            .Where(x => Db.Tickets.Any(t => t.Id == x.TicketId && t.CompanyCen == company.Cen && t.Status == "PAID"))
            .GroupBy(x => x.ProductCen)
            .Select(g => new { productCen = g.Key, quantity = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.quantity)
            .Take(10)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("kds-status")]
    public async Task<IActionResult> KdsStatus(string companyCen)
    {
        var company = await FindOrCreateCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var grouped = await Db.CommandItems
            .Where(x => Db.Commands.Any(c => c.Id == x.CommandId && c.CompanyCen == company.Cen))
            .GroupBy(x => x.Status)
            .Select(g => new { status = g.Key, count = g.Count() })
            .ToListAsync();

        return Ok(grouped);
    }
}
