using Sales.Api.Application.Dtos;

namespace Sales.Api.Application.Abstractions;

public interface IInventoryCatalogClient
{
    Task<PagedResultDto<CatalogProductDto>?> GetSellableProductsAsync(string companyCen, string? search, string? categoryCen, string? warehouseCen, bool onlyAvailable, int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<CatalogProductDto>> LookupProductsAsync(string companyCen, IReadOnlyList<string> productCens, CancellationToken cancellationToken);
    Task<bool> ConsumeStockAsync(string companyCen, string productCen, decimal quantity, CancellationToken cancellationToken);
    Task<bool> ValidateStockAsync(string companyCen, string productCen, decimal quantity, CancellationToken cancellationToken);
}
