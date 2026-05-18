namespace Sales.Api.Domain.Entities;

public sealed class CommandItem
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public int CommandId { get; set; }
    public Guid CommandCen { get; set; }
    public int TicketItemId { get; set; }
    public Guid TicketItemCen { get; set; }
    public int ProductId { get; set; }
    public Guid ProductCen { get; set; }
    public decimal Quantity { get; set; }
    public string Status { get; set; } = "PENDING";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
