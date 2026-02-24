using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intec.Banking.FinancialInstitutions.Migrations
{
    /// <inheritdoc />
    public partial class Add_Audit_And_SoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "FinancialInstitutions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "FinancialInstitutions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "FinancialInstitutions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "FinancialInstitutions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FinancialInstitutions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "FinancialInstitutions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "FinancialInstitutions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FinancialInstitutions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "FinancialInstitutions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "FinancialInstitutions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "FinancialInstitutions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "FinancialInstitutions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "FinancialInstitutions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "FinancialInstitutions");
        }
    }
}
