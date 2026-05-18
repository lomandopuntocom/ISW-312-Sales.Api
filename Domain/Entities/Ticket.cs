namespace Sales.Api.Domain.Entities;

public sealed class Ticket
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public int CompanyId { get; set; }
    public Guid CompanyCen { get; set; }
    public int LocationId { get; set; }
    public Guid LocationCen { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public int? VendorId { get; set; }
    public Guid? VendorCen { get; set; }
    public string? TableCode { get; set; }
    public string Status { get; set; } = "OPEN";
    public List<TicketItem> Items { get; set; } = [];
}
