namespace Sales.Api.Domain.Entities;

public sealed class TicketItem
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public int TicketId { get; set; }
    public Guid TicketCen { get; set; }
    public int ProductId { get; set; }
    public Guid ProductCen { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public string Status { get; set; } = "PENDING";
    public string? Notes { get; set; }
}
