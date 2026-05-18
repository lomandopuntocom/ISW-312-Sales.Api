namespace Sales.Api.Domain.Entities;

public sealed class Payment
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public int TicketId { get; set; }
    public Guid TicketCen { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public string? PaidBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
