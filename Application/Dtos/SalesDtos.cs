namespace Sales.Api.Application.Dtos;

public sealed record PagedResultDto<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
public sealed record CatalogProductDto(string Cen, string Code, string Sku, string Name, decimal Price, bool Active, bool IsOutOfStock, string? StationCode, bool TrackStock = false);
public sealed record WaiterDto(string Cen, string Name, string? Email, string? Phone);
public sealed record TicketDto(string Cen, string TicketNumber, string Status, string? TableCode, string? WaiterCen, int ItemCount);
public sealed record TicketItemDto(string Cen, string ProductCen, string ProductCode, decimal Quantity, decimal UnitPrice, string Status, string? Notes);
public sealed record TicketTotalsDto(decimal Subtotal, decimal Tax, decimal Total);
public sealed record KdsTeamDto(string Cen, string Code, string Name, string StationType);
public sealed record KdsItemDto(string TicketItemCen, string ProductCen, decimal Quantity, string Status, string? Notes);
public sealed record CreateTicketRequest(string? LocationCen, string? TableCode);
public sealed record AddTicketItemRequest(string ProductCen, decimal Quantity, decimal UnitPrice, string? Notes);
public sealed record UpdateTicketItemRequest(decimal Quantity, string? Notes, string? Status);
public sealed record AssignWaiterRequest(string WaiterCen);
public sealed record UpdateKdsStatusRequest(string Status);
public sealed record PaymentRequest(string PaymentMethod, decimal Amount, string? Reference, string? PaidBy);
public sealed record TaxConfigurationDto(decimal TaxRate);
