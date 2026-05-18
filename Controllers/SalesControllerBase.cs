using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sales.Api.Domain.Entities;
using Sales.Api.Infrastructure.Persistence;

namespace Sales.Api.Controllers;

public abstract class SalesControllerBase(SalesDbContext db) : ControllerBase
{
    protected SalesDbContext Db { get; } = db;
    protected static bool TryParseCen(string value, out Guid cen) => Guid.TryParse(value, out cen);

    protected async Task<SalesCompany?> FindOrCreateCompanyAsync(string companyCen)
    {
        if (!TryParseCen(companyCen, out var cen)) return null;

        var company = await Db.Companies.FirstOrDefaultAsync(x => x.Cen == cen);
        if (company is not null) return company;

        company = new SalesCompany { Cen = cen, Name = $"Company {companyCen}" };
        Db.Companies.Add(company);
        await Db.SaveChangesAsync();
        return company;
    }
}
