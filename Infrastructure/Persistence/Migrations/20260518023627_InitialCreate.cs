using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sales.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sales");

            migrationBuilder.CreateTable(
                name: "command_stations",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cen = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<int>(type: "integer", nullable: false),
                    company_cen = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    station_type = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_command_stations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "commands",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cen = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<int>(type: "integer", nullable: false),
                    company_cen = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<int>(type: "integer", nullable: false),
                    location_cen = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<int>(type: "integer", nullable: false),
                    ticket_cen = table.Column<Guid>(type: "uuid", nullable: false),
                    station_id = table.Column<int>(type: "integer", nullable: false),
                    station_cen = table.Column<Guid>(type: "uuid", nullable: false),
                    command_number = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    is_reorder = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ready_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Company",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cen = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cen = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CompanyCen = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cen = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    TicketCen = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    PaidBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tax_configuration",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cen = table.Column<Guid>(type: "uuid", nullable: false),
                    company_cen = table.Column<Guid>(type: "uuid", nullable: false),
                    tax_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tax_configuration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cen = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CompanyCen = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    LocationCen = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketNumber = table.Column<string>(type: "text", nullable: false),
                    VendorId = table.Column<int>(type: "integer", nullable: true),
                    VendorCen = table.Column<Guid>(type: "uuid", nullable: true),
                    TableCode = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendor",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cen = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CompanyCen = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    IsWaiter = table.Column<bool>(type: "boolean", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "command_items",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cen = table.Column<Guid>(type: "uuid", nullable: false),
                    command_id = table.Column<int>(type: "integer", nullable: false),
                    command_cen = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_item_id = table.Column<int>(type: "integer", nullable: false),
                    ticket_item_cen = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    product_cen = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_command_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_command_items_commands_command_id",
                        column: x => x.command_id,
                        principalSchema: "sales",
                        principalTable: "commands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketItem",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cen = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    TicketCen = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ProductCen = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketItem_Ticket_TicketId",
                        column: x => x.TicketId,
                        principalSchema: "sales",
                        principalTable: "Ticket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_command_items_cen",
                schema: "sales",
                table: "command_items",
                column: "cen",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_command_items_command_id",
                schema: "sales",
                table: "command_items",
                column: "command_id");

            migrationBuilder.CreateIndex(
                name: "IX_command_stations_cen",
                schema: "sales",
                table: "command_stations",
                column: "cen",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_commands_cen",
                schema: "sales",
                table: "commands",
                column: "cen",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_Cen",
                schema: "sales",
                table: "Company",
                column: "Cen",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_Cen",
                schema: "sales",
                table: "Location",
                column: "Cen",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_Cen",
                schema: "sales",
                table: "Payment",
                column: "Cen",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tax_configuration_company_cen",
                schema: "sales",
                table: "tax_configuration",
                column: "company_cen",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_Cen",
                schema: "sales",
                table: "Ticket",
                column: "Cen",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketItem_Cen",
                schema: "sales",
                table: "TicketItem",
                column: "Cen",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketItem_TicketId",
                schema: "sales",
                table: "TicketItem",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_Cen",
                schema: "sales",
                table: "Vendor",
                column: "Cen",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "command_items",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "command_stations",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "Company",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "Location",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "Payment",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "tax_configuration",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "TicketItem",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "Vendor",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "commands",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "Ticket",
                schema: "sales");
        }
    }
}
