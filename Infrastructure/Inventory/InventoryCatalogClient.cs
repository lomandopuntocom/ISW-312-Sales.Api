using System.Net.Http.Json;
using Sales.Api.Application.Abstractions;
using Sales.Api.Application.Dtos;

namespace Sales.Api.Infrastructure.Inventory;

public sealed class InventoryCatalogClient(HttpClient httpClient) : IInventoryCatalogClient
{
    public Task<PagedResultDto<CatalogProductDto>?> GetSellableProductsAsync(
        string companyCen,
        string? search,
        string? categoryCen,
        string? warehouseCen,
        bool onlyAvailable,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = new List<string>
        {
            $"onlyAvailable={onlyAvailable}",
            $"page={page}",
            $"pageSize={pageSize}"
        };
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(categoryCen)) query.Add($"categoryCen={Uri.EscapeDataString(categoryCen)}");
        if (!string.IsNullOrWhiteSpace(warehouseCen)) query.Add($"warehouseCen={Uri.EscapeDataString(warehouseCen)}");

        return httpClient.GetFromJsonAsync<PagedResultDto<CatalogProductDto>>(
            $"/api/inventory/companies/{companyCen}/sellable-products?{string.Join("&", query)}",
            cancellationToken);
    }

    public async Task<IReadOnlyList<CatalogProductDto>> LookupProductsAsync(
        string companyCen,
        IReadOnlyList<string> productCens,
        CancellationToken cancellationToken)
    {
        if (productCens.Count == 0) return Array.Empty<CatalogProductDto>();

        var response = await httpClient.PostAsJsonAsync(
            $"/api/inventory/companies/{companyCen}/products/lookup",
            new { productCens },
            cancellationToken);

        if (!response.IsSuccessStatusCode) return Array.Empty<CatalogProductDto>();

        return await response.Content.ReadFromJsonAsync<IReadOnlyList<CatalogProductDto>>(cancellationToken)
            ?? Array.Empty<CatalogProductDto>();
    }

    public async Task<bool> ConsumeStockAsync(string companyCen, string productCen, decimal quantity, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"/api/inventory/companies/{companyCen}/stock/consume",
            new { productCen, quantity, reference = "SALES", notes = "Ticket payment" },
            cancellationToken);

        return response.IsSuccessStatusCode;
    }
}
