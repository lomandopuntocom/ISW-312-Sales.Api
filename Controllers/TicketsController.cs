using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sales.Api.Application.Abstractions;
using Sales.Api.Application.Dtos;
using Sales.Api.Domain.Entities;
using Sales.Api.Infrastructure.Persistence;

namespace Sales.Api.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/tickets")]
public sealed class TicketsController(SalesDbContext db, IInventoryCatalogClient inventoryClient) : SalesControllerBase(db)
{
    [HttpGet]
    public async Task<IActionResult> GetTickets(string companyCen, [FromQuery] string? status = null)
    {
        var company = await FindOrCreateCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var query = Db.Tickets.Include(x => x.Items).Where(x => x.CompanyCen == company.Cen);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);

        var tickets = await query
            .OrderByDescending(x => x.Id)
            .Select(x => new TicketDto(x.Cen.ToString(), x.TicketNumber, x.Status, x.TableCode, x.VendorCen == null ? null : x.VendorCen.ToString(), x.Items.Count))
            .ToListAsync();

        return Ok(tickets);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket(string companyCen, CreateTicketRequest request)
    {
        var company = await FindOrCreateCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var location = await ResolveLocationAsync(company, request.LocationCen);
        var next = await Db.Tickets.CountAsync(x => x.CompanyCen == company.Cen) + 1;
        var ticket = new Ticket
        {
            CompanyId = company.Id,
            CompanyCen = company.Cen,
            LocationId = location.Id,
            LocationCen = location.Cen,
            TicketNumber = $"TIC-{next:00000}",
            TableCode = request.TableCode,
            Status = "OPEN"
        };

        Db.Tickets.Add(ticket);
        await Db.SaveChangesAsync();
        return Ok(ToDto(ticket));
    }

    [HttpGet("{ticketCen}/items")]
    public async Task<IActionResult> GetItems(string companyCen, string ticketCen, CancellationToken cancellationToken)
    {
        var ticket = await FindTicketAsync(companyCen, ticketCen);
        if (ticket is null) return NotFound();

        var productCodes = await ResolveProductCodesAsync(companyCen, ticket.Items.Select(x => x.ProductCen.ToString()).ToList(), cancellationToken);
        var items = ticket.Items
            .Select(x => ToItemDto(x, productCodes))
            .ToList();

        return Ok(items);
    }

    [HttpPost("{ticketCen}/items")]
    public async Task<IActionResult> AddItem(string companyCen, string ticketCen, AddTicketItemRequest request, CancellationToken cancellationToken)
    {
        var ticket = await FindTicketAsync(companyCen, ticketCen);
        if (ticket is null) return NotFound();
        if (ticket.Status != "OPEN") return Conflict("Ticket is not open.");
        if (!TryParseCen(request.ProductCen, out var productCen)) return BadRequest("Invalid product CEN.");
        if (!TryNormalizeQuantity(request.Quantity, out var quantity, out var quantityError)) return BadRequest(quantityError);

        var existing = ticket.Items.FirstOrDefault(x => x.ProductCen == productCen);
        if (existing is not null)
        {
            existing.Quantity += quantity;
            if (!string.IsNullOrWhiteSpace(request.Notes)) existing.Notes = request.Notes;
            await Db.SaveChangesAsync();

            var mergedCodes = await ResolveProductCodesAsync(companyCen, [existing.ProductCen.ToString()], cancellationToken);
            return Ok(ToItemDto(existing, mergedCodes));
        }

        var item = new TicketItem
        {
            TicketId = ticket.Id,
            TicketCen = ticket.Cen,
            ProductCen = productCen,
            Quantity = quantity,
            UnitPrice = request.UnitPrice,
            Notes = request.Notes,
            Status = "PENDING"
        };

        Db.TicketItems.Add(item);
        await Db.SaveChangesAsync();

        var productCodes = await ResolveProductCodesAsync(companyCen, [item.ProductCen.ToString()], cancellationToken);
        return Ok(ToItemDto(item, productCodes));
    }

    [HttpPatch("{ticketCen}/items/{ticketItemCen}")]
    public async Task<IActionResult> UpdateItem(string companyCen, string ticketCen, string ticketItemCen, UpdateTicketItemRequest request, CancellationToken cancellationToken)
    {
        var ticket = await FindTicketAsync(companyCen, ticketCen);
        if (ticket is null || !TryParseCen(ticketItemCen, out var itemCen)) return NotFound();

        var item = ticket.Items.FirstOrDefault(x => x.Cen == itemCen);
        if (item is null) return NotFound();

        if (request.Quantity < 1)
        {
            Db.TicketItems.Remove(item);
            await Db.SaveChangesAsync();
            return NoContent();
        }

        if (!TryNormalizeQuantity(request.Quantity, out var quantity, out var quantityError)) return BadRequest(quantityError);

        item.Quantity = quantity;
        item.Notes = request.Notes;
        item.Status = request.Status ?? item.Status;
        await Db.SaveChangesAsync();

        var productCodes = await ResolveProductCodesAsync(companyCen, [item.ProductCen.ToString()], cancellationToken);
        return Ok(ToItemDto(item, productCodes));
    }

    [HttpPost("{ticketCen}/items/{ticketItemCen}/resend")]
    public async Task<IActionResult> ResendItem(string companyCen, string ticketCen, string ticketItemCen)
    {
        var ticket = await FindTicketAsync(companyCen, ticketCen);
        if (ticket is null || !TryParseCen(ticketItemCen, out var itemCen)) return NotFound();

        var item = ticket.Items.FirstOrDefault(x => x.Cen == itemCen);
        if (item is null) return NotFound();

        item.Status = "PENDING";
        await Db.SaveChangesAsync();
        return Ok(new { itemCen = item.Cen, item.Status });
    }

    [HttpPost("{ticketCen}/send")]
    public async Task<IActionResult> SendToKitchen(string companyCen, string ticketCen)
    {
        var ticket = await FindTicketAsync(companyCen, ticketCen);
        if (ticket is null) return NotFound();

        foreach (var item in ticket.Items.Where(x => x.Status == "PENDING"))
            item.Status = "SENT";

        await Db.SaveChangesAsync();
        return Ok(new { ticketCen = ticket.Cen, status = "SENT" });
    }

    [HttpPut("{ticketCen}/waiter")]
    public async Task<IActionResult> AssignWaiter(string companyCen, string ticketCen, AssignWaiterRequest request)
    {
        var ticket = await FindTicketAsync(companyCen, ticketCen);
        if (ticket is null || !TryParseCen(request.WaiterCen, out var waiterCen)) return NotFound();

        var waiter = await Db.Vendors.FirstOrDefaultAsync(x => x.CompanyCen == ticket.CompanyCen && x.Cen == waiterCen && x.IsWaiter);
        if (waiter is null) return NotFound("Waiter not found.");

        ticket.VendorId = waiter.Id;
        ticket.VendorCen = waiter.Cen;
        await Db.SaveChangesAsync();
        return Ok(ToDto(ticket));
    }

    [HttpPost("{ticketCen}/payment")]
    public async Task<IActionResult> Pay(string companyCen, string ticketCen, PaymentRequest request, CancellationToken cancellationToken)
    {
        var ticket = await FindTicketAsync(companyCen, ticketCen);
        if (ticket is null) return NotFound();
        if (ticket.Status != "OPEN") return Conflict("Ticket is not open.");

        foreach (var item in ticket.Items)
        {
            var consumed = await inventoryClient.ConsumeStockAsync(companyCen, item.ProductCen.ToString(), item.Quantity, cancellationToken);
            if (!consumed) return Conflict($"Insufficient stock or inventory error for product {item.ProductCen}.");
        }

        Db.Payments.Add(new Payment
        {
            TicketId = ticket.Id,
            TicketCen = ticket.Cen,
            PaymentMethod = request.PaymentMethod,
            Amount = request.Amount,
            Reference = request.Reference,
            PaidBy = request.PaidBy
        });

        ticket.Status = "PAID";
        await Db.SaveChangesAsync();
        return Ok(new { ticketCen = ticket.Cen, ticket.Status });
    }

    [HttpPost("{ticketCen}/cancel")]
    public async Task<IActionResult> Cancel(string companyCen, string ticketCen)
    {
        var ticket = await FindTicketAsync(companyCen, ticketCen);
        if (ticket is null) return NotFound();
        if (ticket.Status == "PAID") return Conflict("Paid tickets cannot be cancelled.");

        ticket.Status = "CANCELLED";
        await Db.SaveChangesAsync();
        return Ok(ToDto(ticket));
    }

    [HttpGet("{ticketCen}/totals")]
    public async Task<IActionResult> Totals(string companyCen, string ticketCen)
    {
        var ticket = await FindTicketAsync(companyCen, ticketCen);
        if (ticket is null) return NotFound();

        var subtotal = ticket.Items.Sum(x => x.Quantity * x.UnitPrice);
        var taxRate = await Db.TaxConfigurations
            .Where(x => x.CompanyCen == ticket.CompanyCen)
            .Select(x => (decimal?)x.TaxRate)
            .FirstOrDefaultAsync() ?? 0.18m;

        var tax = subtotal * taxRate;
        return Ok(new TicketTotalsDto(subtotal, tax, subtotal + tax));
    }

    [HttpGet("{ticketCen}/print")]
    public async Task<IActionResult> Print(string companyCen, string ticketCen)
    {
        var ticket = await FindTicketAsync(companyCen, ticketCen);
        return ticket is null ? NotFound() : Ok(new { ticketCen = ticket.Cen, ticket.TicketNumber, ticket.Status });
    }

    private async Task<Ticket?> FindTicketAsync(string companyCen, string ticketCen)
    {
        var company = await FindOrCreateCompanyAsync(companyCen);
        if (company is null || !TryParseCen(ticketCen, out var cen)) return null;
        return await Db.Tickets.Include(x => x.Items).FirstOrDefaultAsync(x => x.CompanyCen == company.Cen && x.Cen == cen);
    }

    private async Task<Domain.Entities.SalesLocation> ResolveLocationAsync(Domain.Entities.SalesCompany company, string? locationCen)
    {
        Domain.Entities.SalesLocation? location = null;

        if (TryParseCen(locationCen ?? string.Empty, out var requestedLocation))
            location = await Db.Locations.FirstOrDefaultAsync(x => x.CompanyCen == company.Cen && x.Cen == requestedLocation);

        location ??= await Db.Locations.FirstOrDefaultAsync(x => x.CompanyCen == company.Cen);

        if (location is not null) return location;

        location = new Domain.Entities.SalesLocation
        {
            CompanyId = company.Id,
            CompanyCen = company.Cen,
            Name = "Principal"
        };

        Db.Locations.Add(location);
        await Db.SaveChangesAsync();
        return location;
    }

    private static TicketDto ToDto(Ticket x) => new(x.Cen.ToString(), x.TicketNumber, x.Status, x.TableCode, x.VendorCen?.ToString(), x.Items.Count);

    private async Task<Dictionary<string, string>> ResolveProductCodesAsync(
        string companyCen,
        IReadOnlyList<string> productCens,
        CancellationToken cancellationToken)
    {
        var products = await inventoryClient.LookupProductsAsync(companyCen, productCens, cancellationToken);
        return products.ToDictionary(x => x.Cen, x => x.Code, StringComparer.OrdinalIgnoreCase);
    }

    private static TicketItemDto ToItemDto(TicketItem item, IReadOnlyDictionary<string, string> productCodes)
    {
        var productCen = item.ProductCen.ToString();
        var productCode = productCodes.TryGetValue(productCen, out var code) ? code : productCen;
        return new TicketItemDto(item.Cen.ToString(), productCen, productCode, item.Quantity, item.UnitPrice, item.Status, item.Notes);
    }

    private static bool TryNormalizeQuantity(decimal quantity, out decimal normalized, out string? error)
    {
        if (quantity != Math.Floor(quantity) || quantity < 1)
        {
            normalized = 0;
            error = "Quantity must be a positive whole number.";
            return false;
        }

        normalized = quantity;
        error = null;
        return true;
    }
}
