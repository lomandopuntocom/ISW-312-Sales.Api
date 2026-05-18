namespace Sales.Api.Domain.Entities;

public sealed class Command
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public int CompanyId { get; set; }
    public Guid CompanyCen { get; set; }
    public int LocationId { get; set; }
    public Guid LocationCen { get; set; }
    public int TicketId { get; set; }
    public Guid TicketCen { get; set; }
    public int StationId { get; set; }
    public Guid StationCen { get; set; }
    public string CommandNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "SENT";
    public bool IsReorder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadyAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<CommandItem> Items { get; set; } = [];
}
