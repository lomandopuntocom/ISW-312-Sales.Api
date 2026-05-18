namespace Sales.Api.Domain.Entities;

public sealed class TaxConfiguration
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public Guid CompanyCen { get; set; }
    public decimal TaxRate { get; set; } = 0.18m;
}
