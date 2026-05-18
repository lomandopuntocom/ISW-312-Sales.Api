namespace Sales.Api.Domain.Entities;

public sealed class SalesCompany
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}
