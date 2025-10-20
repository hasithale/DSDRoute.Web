using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSDRoute.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxPercentageAndInvoiceDiscountToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InvoiceDiscount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxPercentage",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceDiscount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TaxPercentage",
                table: "Orders");
        }
    }
}
