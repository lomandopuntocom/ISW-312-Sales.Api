using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sales.Api.Application.Dtos;
using Sales.Api.Infrastructure.Persistence;

namespace Sales.Api.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/kds")]
public sealed class KdsController(SalesDbContext db) : SalesControllerBase(db)
{
    [HttpGet("teams")]
    public async Task<IActionResult> GetTeams(string companyCen)
    {
        var company = await FindOrCreateCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var teams = await Db.CommandStations
            .Where(x => x.CompanyCen == company.Cen && x.Active)
            .OrderBy(x => x.Name)
            .Select(x => new KdsTeamDto(x.Cen.ToString(), x.Code, x.Name, x.StationType))
            .ToListAsync();

        return Ok(teams);
    }

    [HttpGet("teams/{teamCen}/items")]
    public async Task<IActionResult> GetTeamItems(string companyCen, string teamCen)
    {
        var company = await FindOrCreateCompanyAsync(companyCen);
        if (company is null || !TryParseCen(teamCen, out var team)) return NotFound();

        var items = await Db.CommandItems
            .Where(x => Db.Commands.Any(c => c.Id == x.CommandId && c.CompanyCen == company.Cen && c.StationCen == team) && x.Status != "READY")
            .OrderBy(x => x.CreatedAt)
            .Select(x => new KdsItemDto(x.TicketItemCen.ToString(), x.ProductCen.ToString(), x.Quantity, x.Status, x.Notes))
            .ToListAsync();

        return Ok(items);
    }
}
