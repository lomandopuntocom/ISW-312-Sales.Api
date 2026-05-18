namespace Sales.Api.Domain.Entities;

public sealed class SalesLocation
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public int CompanyId { get; set; }
    public Guid CompanyCen { get; set; }
    public string Name { get; set; } = string.Empty;
}
