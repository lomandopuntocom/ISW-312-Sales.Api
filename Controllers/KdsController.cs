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

        var commands = await Db.Commands
            .Include(x => x.Items)
            .Where(x => x.CompanyCen == company.Cen && x.StationCen == team && x.Items.Any(i => i.Status != "READY"))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        var result = commands.Select(c => new
        {
            id = c.Cen.ToString(),
            ticketId = c.TicketCen.ToString(),
            fechaEnvio = c.SentAt ?? c.CreatedAt,
            items = c.Items.Where(i => i.Status != "READY").Select(i => new
            {
                id = i.TicketItemCen.ToString(),
                producto = i.ProductCen.ToString(),
                cantidad = i.Quantity,
                estado = i.Status,
                nota = i.Notes
            }).ToList()
        }).ToList();

        return Ok(result);
    }
}
