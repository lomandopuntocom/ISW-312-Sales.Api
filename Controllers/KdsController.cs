using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sales.Api.Application.Dtos;
using Sales.Api.Domain.Entities;
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

    [HttpPost("teams")]
    public async Task<IActionResult> CreateTeam(string companyCen, CreateKdsTeamContractRequest request)
    {
        var company = await FindOrCreateCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var count = await Db.CommandStations.CountAsync(x => x.CompanyCen == company.Cen);
        var code = $"KDS-{(count + 1):D5}";

        var station = new CommandStation
        {
            CompanyId = company.Id,
            CompanyCen = company.Cen,
            Code = code,
            Name = request.Name,
            StationType = "KDS",
            Active = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Db.CommandStations.Add(station);
        await Db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTeams),
            new { companyCen },
            new KdsTeamContractResponse(station.Cen.ToString(), station.Name, request.CategoryCens ?? Array.Empty<string>()));
    }
}

