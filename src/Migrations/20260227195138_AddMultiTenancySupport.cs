using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intec.Banking.FinancialInstitutions.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "FinancialInstitutions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_FinancialInstitutions_TenantId",
                table: "FinancialInstitutions",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinancialInstitutions_TenantId",
                table: "FinancialInstitutions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "FinancialInstitutions");
        }
    }
}
