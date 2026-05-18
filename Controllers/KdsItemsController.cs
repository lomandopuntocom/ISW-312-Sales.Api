using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sales.Api.Application.Dtos;
using Sales.Api.Infrastructure.Persistence;

namespace Sales.Api.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/kds/items")]
public sealed class KdsItemsController(SalesDbContext db) : SalesControllerBase(db)
{
    [HttpPatch("{ticketItemCen}/status")]
    public async Task<IActionResult> UpdateStatus(string companyCen, string ticketItemCen, UpdateKdsStatusRequest request)
    {
        var company = await FindOrCreateCompanyAsync(companyCen);
        if (company is null || !TryParseCen(ticketItemCen, out var itemCen)) return NotFound();

        var item = await Db.CommandItems
            .FirstOrDefaultAsync(x => x.TicketItemCen == itemCen && Db.Commands.Any(c => c.Id == x.CommandId && c.CompanyCen == company.Cen));

        if (item is null) return NotFound();

        item.Status = request.Status;
        item.UpdatedAt = DateTime.UtcNow;
        await Db.SaveChangesAsync();
        return Ok(new { ticketItemCen = item.TicketItemCen, item.Status });
    }
}
