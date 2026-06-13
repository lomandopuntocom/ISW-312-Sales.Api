using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sales.Api.Application.Dtos;
using Sales.Api.Domain.Entities;
using Sales.Api.Infrastructure.Persistence;

namespace Sales.Api.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/waiters")]
public sealed class WaitersController(SalesDbContext db) : SalesControllerBase(db)
{
    [HttpGet]
    public async Task<IActionResult> GetWaiters(string companyCen)
    {
        var company = await FindOrCreateCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var waiters = await Db.Vendors
            .Where(x => x.CompanyCen == company.Cen && x.IsWaiter && x.Active)
            .OrderBy(x => x.Name)
            .Select(x => new WaiterDto(x.Cen.ToString(), x.Name, x.Email, x.Phone))
            .ToListAsync();

        return Ok(waiters);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWaiter(string companyCen, CreateWaiterContractRequest request)
    {
        var company = await FindOrCreateCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var waiter = new Vendor
        {
            CompanyId = company.Id,
            CompanyCen = company.Cen,
            Name = request.Name,
            IsWaiter = true,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Db.Vendors.Add(waiter);
        await Db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetWaiters),
            new { companyCen },
            new WaiterContractResponse(waiter.Cen.ToString(), waiter.Name));
    }
}

