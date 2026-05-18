using Microsoft.EntityFrameworkCore;
using Sales.Api.Domain.Entities;

namespace Sales.Api.Infrastructure.Persistence;

public sealed class SalesDbContext(DbContextOptions<SalesDbContext> options) : DbContext(options)
{
    public DbSet<SalesCompany> Companies => Set<SalesCompany>();
    public DbSet<SalesLocation> Locations => Set<SalesLocation>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketItem> TicketItems => Set<TicketItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CommandStation> CommandStations => Set<CommandStation>();
    public DbSet<Command> Commands => Set<Command>();
    public DbSet<CommandItem> CommandItems => Set<CommandItem>();
    public DbSet<TaxConfiguration> TaxConfigurations => Set<TaxConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sales");

        modelBuilder.Entity<SalesCompany>(entity =>
        {
            entity.ToTable("Company");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Cen).HasColumnName("Cen");
            entity.Property(x => x.Name).HasColumnName("Name");
        });

        modelBuilder.Entity<SalesLocation>(entity =>
        {
            entity.ToTable("Location");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Cen).HasColumnName("Cen");
            entity.Property(x => x.CompanyId).HasColumnName("CompanyId");
            entity.Property(x => x.CompanyCen).HasColumnName("CompanyCen");
            entity.Property(x => x.Name).HasColumnName("Name");
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.ToTable("Vendor");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Cen).HasColumnName("Cen");
            entity.Property(x => x.CompanyId).HasColumnName("CompanyId");
            entity.Property(x => x.CompanyCen).HasColumnName("CompanyCen");
            entity.Property(x => x.Name).HasColumnName("Name");
            entity.Property(x => x.Email).HasColumnName("Email");
            entity.Property(x => x.Phone).HasColumnName("Phone");
            entity.Property(x => x.IsWaiter).HasColumnName("IsWaiter");
            entity.Property(x => x.Active).HasColumnName("Active");
            entity.Property(x => x.CreatedAt).HasColumnName("CreatedAt");
            entity.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Ticket");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Cen).HasColumnName("Cen");
            entity.Property(x => x.CompanyId).HasColumnName("CompanyId");
            entity.Property(x => x.CompanyCen).HasColumnName("CompanyCen");
            entity.Property(x => x.LocationId).HasColumnName("LocationId");
            entity.Property(x => x.LocationCen).HasColumnName("LocationCen");
            entity.Property(x => x.TicketNumber).HasColumnName("TicketNumber");
            entity.Property(x => x.VendorId).HasColumnName("VendorId");
            entity.Property(x => x.VendorCen).HasColumnName("VendorCen");
            entity.Property(x => x.TableCode).HasColumnName("TableCode");
            entity.Property(x => x.Status).HasColumnName("Status");
            entity.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.TicketId);
        });

        modelBuilder.Entity<TicketItem>(entity =>
        {
            entity.ToTable("TicketItem");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Cen).HasColumnName("Cen");
            entity.Property(x => x.TicketId).HasColumnName("TicketId");
            entity.Property(x => x.TicketCen).HasColumnName("TicketCen");
            entity.Property(x => x.ProductId).HasColumnName("ProductId");
            entity.Property(x => x.ProductCen).HasColumnName("ProductCen");
            entity.Property(x => x.Quantity).HasColumnName("Quantity").HasPrecision(12, 2);
            entity.Property(x => x.UnitPrice).HasColumnName("UnitPrice").HasPrecision(12, 2);
            entity.Property(x => x.Status).HasColumnName("Status");
            entity.Property(x => x.Notes).HasColumnName("Notes");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payment");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Id).HasColumnName("Id");
            entity.Property(x => x.Cen).HasColumnName("Cen");
            entity.Property(x => x.TicketId).HasColumnName("TicketId");
            entity.Property(x => x.TicketCen).HasColumnName("TicketCen");
            entity.Property(x => x.PaymentMethod).HasColumnName("PaymentMethod");
            entity.Property(x => x.Amount).HasColumnName("Amount").HasPrecision(12, 2);
            entity.Property(x => x.Reference).HasColumnName("Reference");
            entity.Property(x => x.PaidBy).HasColumnName("PaidBy");
            entity.Property(x => x.CreatedAt).HasColumnName("CreatedAt");
            entity.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt");
        });

        modelBuilder.Entity<CommandStation>(entity =>
        {
            entity.ToTable("command_stations");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.CompanyId).HasColumnName("company_id");
            entity.Property(x => x.CompanyCen).HasColumnName("company_cen");
            entity.Property(x => x.Code).HasColumnName("code");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.StationType).HasColumnName("station_type");
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.Active).HasColumnName("active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Command>(entity =>
        {
            entity.ToTable("commands");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.CompanyId).HasColumnName("company_id");
            entity.Property(x => x.CompanyCen).HasColumnName("company_cen");
            entity.Property(x => x.LocationId).HasColumnName("location_id");
            entity.Property(x => x.LocationCen).HasColumnName("location_cen");
            entity.Property(x => x.TicketId).HasColumnName("ticket_id");
            entity.Property(x => x.TicketCen).HasColumnName("ticket_cen");
            entity.Property(x => x.StationId).HasColumnName("station_id");
            entity.Property(x => x.StationCen).HasColumnName("station_cen");
            entity.Property(x => x.CommandNumber).HasColumnName("command_number");
            entity.Property(x => x.Status).HasColumnName("status");
            entity.Property(x => x.IsReorder).HasColumnName("is_reorder");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.SentAt).HasColumnName("sent_at");
            entity.Property(x => x.ReadyAt).HasColumnName("ready_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.CommandId);
        });

        modelBuilder.Entity<CommandItem>(entity =>
        {
            entity.ToTable("command_items");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.CommandId).HasColumnName("command_id");
            entity.Property(x => x.CommandCen).HasColumnName("command_cen");
            entity.Property(x => x.TicketItemId).HasColumnName("ticket_item_id");
            entity.Property(x => x.TicketItemCen).HasColumnName("ticket_item_cen");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.ProductCen).HasColumnName("product_cen");
            entity.Property(x => x.Quantity).HasColumnName("quantity").HasPrecision(12, 2);
            entity.Property(x => x.Status).HasColumnName("status");
            entity.Property(x => x.Notes).HasColumnName("notes");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<TaxConfiguration>(entity =>
        {
            entity.ToTable("tax_configuration");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CompanyCen).IsUnique();
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.CompanyCen).HasColumnName("company_cen");
            entity.Property(x => x.TaxRate).HasColumnName("tax_rate").HasPrecision(5, 2);
        });
    }
}
