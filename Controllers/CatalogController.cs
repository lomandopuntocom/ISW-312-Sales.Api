using Microsoft.AspNetCore.Mvc;
using Sales.Api.Application.Abstractions;

namespace Sales.Api.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/catalog")]
public sealed class CatalogController(IInventoryCatalogClient inventoryClient) : ControllerBase
{
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        string companyCen,
        [FromQuery] string? search,
        [FromQuery] string? categoryCen,
        [FromQuery] string? warehouseCen,
        [FromQuery] bool onlyAvailable = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var products = await inventoryClient.GetSellableProductsAsync(companyCen, search, categoryCen, warehouseCen, onlyAvailable, page, pageSize, cancellationToken);
        return products is null ? StatusCode(502, "Inventory API did not return catalog data.") : Ok(products);
    }
}
