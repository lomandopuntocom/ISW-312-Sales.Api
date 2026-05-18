namespace Sales.Api.Domain.Entities;

public sealed class Vendor
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public int CompanyId { get; set; }
    public Guid CompanyCen { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsWaiter { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
