using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sales.Api.Application.Dtos;
using Sales.Api.Domain.Entities;
using Sales.Api.Infrastructure.Persistence;

namespace Sales.Api.Controllers;

[ApiController]
[Route("api/sales")]
public sealed class SettingsController(SalesDbContext db) : SalesControllerBase(db)
{
    [HttpGet("payment-methods")]
    public IActionResult PaymentMethods()
    {
        return Ok(new[] { "CASH", "CARD", "TRANSFER", "QR" });
    }

    [HttpGet("companies/{companyCen}/tax-configuration")]
    public async Task<IActionResult> GetTax(string companyCen)
    {
        if (!TryParseCen(companyCen, out var cen)) return NotFound();
        var tax = await Db.TaxConfigurations.FirstOrDefaultAsync(x => x.CompanyCen == cen);
        return Ok(new TaxConfigurationDto(tax?.TaxRate ?? 0.18m));
    }

    [HttpPut("companies/{companyCen}/tax-configuration")]
    public async Task<IActionResult> UpdateTax(string companyCen, TaxConfigurationDto request)
    {
        if (!TryParseCen(companyCen, out var cen)) return NotFound();

        var tax = await Db.TaxConfigurations.FirstOrDefaultAsync(x => x.CompanyCen == cen);
        if (tax is null)
        {
            tax = new TaxConfiguration { CompanyCen = cen, TaxRate = request.TaxRate };
            Db.TaxConfigurations.Add(tax);
        }
        else
        {
            tax.TaxRate = request.TaxRate;
        }

        await Db.SaveChangesAsync();
        return Ok(new TaxConfigurationDto(tax.TaxRate));
    }
}
