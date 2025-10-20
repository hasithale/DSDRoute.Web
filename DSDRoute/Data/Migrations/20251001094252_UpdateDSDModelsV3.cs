using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSDRoute.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDSDModelsV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditBills_AspNetUsers_CreatedByUserId",
                table: "CreditBills");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_ApprovedByUserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_ReceivedByUserId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Shops_ShopId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Returns_AspNetUsers_ProcessedByUserId",
                table: "Returns");

            migrationBuilder.DropForeignKey(
                name: "FK_Shops_AspNetUsers_CreatedByUserId",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Shops_CreatedByUserId",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Returns_ProcessedByUserId",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ReceivedByUserId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_CreditBills_CreatedByUserId",
                table: "CreditBills");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ProcessedByUserId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "ReceivedByUserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "CreditBills");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CreditBills");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "CreditBills");

            migrationBuilder.DropColumn(
                name: "PaidDate",
                table: "CreditBills");

            migrationBuilder.RenameColumn(
                name: "StockQuantity",
                table: "Products",
                newName: "StockQty");

            migrationBuilder.RenameColumn(
                name: "ApprovedDate",
                table: "Orders",
                newName: "DeliveryDate");

            migrationBuilder.RenameColumn(
                name: "ApprovedByUserId",
                table: "Orders",
                newName: "DeliveredById");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_ApprovedByUserId",
                table: "Orders",
                newName: "IX_Orders_DeliveredById");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Shops",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Shops",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "Shops",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Shops",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Shops",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Shops",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Returns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedById",
                table: "Returns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "Returns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "Returns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessedById",
                table: "Returns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "Returns",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Returns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Returns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "SKU",
                table: "Products",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ShopId",
                table: "Payments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChequeNumber",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RecordedById",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerifiedById",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "CreditBills",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CreditBills",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "CreditBills",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "CreditBills",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CreditBills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Returns_ApprovedById",
                table: "Returns",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_OrderId",
                table: "Returns",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_ProcessedById",
                table: "Returns",
                column: "ProcessedById");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_RecordedById",
                table: "Payments",
                column: "RecordedById");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_VerifiedById",
                table: "Payments",
                column: "VerifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_DeliveredById",
                table: "Orders",
                column: "DeliveredById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_RecordedById",
                table: "Payments",
                column: "RecordedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_VerifiedById",
                table: "Payments",
                column: "VerifiedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Shops_ShopId",
                table: "Payments",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_AspNetUsers_ApprovedById",
                table: "Returns",
                column: "ApprovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_AspNetUsers_ProcessedById",
                table: "Returns",
                column: "ProcessedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_Orders_OrderId",
                table: "Returns",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_DeliveredById",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_RecordedById",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_VerifiedById",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Shops_ShopId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Returns_AspNetUsers_ApprovedById",
                table: "Returns");

            migrationBuilder.DropForeignKey(
                name: "FK_Returns_AspNetUsers_ProcessedById",
                table: "Returns");

            migrationBuilder.DropForeignKey(
                name: "FK_Returns_Orders_OrderId",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Returns_ApprovedById",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Returns_OrderId",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Returns_ProcessedById",
                table: "Returns");

            migrationBuilder.DropIndex(
                name: "IX_Payments_RecordedById",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_VerifiedById",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Contact",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "ProcessedById",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Returns");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RecordedById",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VerificationDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VerifiedById",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CreditBills");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "CreditBills");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "CreditBills");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CreditBills");

            migrationBuilder.RenameColumn(
                name: "StockQty",
                table: "Products",
                newName: "StockQuantity");

            migrationBuilder.RenameColumn(
                name: "DeliveryDate",
                table: "Orders",
                newName: "ApprovedDate");

            migrationBuilder.RenameColumn(
                name: "DeliveredById",
                table: "Orders",
                newName: "ApprovedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_DeliveredById",
                table: "Orders",
                newName: "IX_Orders_ApprovedByUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Shops",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Shops",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "Shops",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Shops",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Shops",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Returns",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ProcessedByUserId",
                table: "Returns",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "SKU",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ShopId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "Payments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChequeNumber",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "decimal(12,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "ReceivedByUserId",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Orders",
                type: "decimal(12,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "OrderItems",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "OrderItems",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "CreditBills",
                type: "decimal(12,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "CreditBills",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CreditBills",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "CreditBills",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidDate",
                table: "CreditBills",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_CreatedByUserId",
                table: "Shops",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_ProcessedByUserId",
                table: "Returns",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ReceivedByUserId",
                table: "Payments",
                column: "ReceivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CreditBills_CreatedByUserId",
                table: "CreditBills",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditBills_AspNetUsers_CreatedByUserId",
                table: "CreditBills",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_ApprovedByUserId",
                table: "Orders",
                column: "ApprovedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_ReceivedByUserId",
                table: "Payments",
                column: "ReceivedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Shops_ShopId",
                table: "Payments",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Returns_AspNetUsers_ProcessedByUserId",
                table: "Returns",
                column: "ProcessedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_AspNetUsers_CreatedByUserId",
                table: "Shops",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
