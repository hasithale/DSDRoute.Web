using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSDRoute.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCreditBillTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "CreditBills");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CreditBills");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "CreditBills");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CreditBills");

            migrationBuilder.RenameColumn(
                name: "PaidAmount",
                table: "CreditBills",
                newName: "CreditAmount");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "CreditBills",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceId",
                table: "CreditBills",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSettled",
                table: "CreditBills",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SalesRepId",
                table: "CreditBills",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SettledDate",
                table: "CreditBills",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditBills_SalesRepId",
                table: "CreditBills",
                column: "SalesRepId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditBills_AspNetUsers_SalesRepId",
                table: "CreditBills",
                column: "SalesRepId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditBills_AspNetUsers_SalesRepId",
                table: "CreditBills");

            migrationBuilder.DropIndex(
                name: "IX_CreditBills_SalesRepId",
                table: "CreditBills");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "CreditBills");

            migrationBuilder.DropColumn(
                name: "IsSettled",
                table: "CreditBills");

            migrationBuilder.DropColumn(
                name: "SalesRepId",
                table: "CreditBills");

            migrationBuilder.DropColumn(
                name: "SettledDate",
                table: "CreditBills");

            migrationBuilder.RenameColumn(
                name: "CreditAmount",
                table: "CreditBills",
                newName: "PaidAmount");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "CreditBills",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "CreditBills",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CreditBills",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "CreditBills",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CreditBills",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
