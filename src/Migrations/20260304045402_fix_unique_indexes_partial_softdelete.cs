using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intec.Banking.FinancialInstitutions.Migrations
{
    /// <inheritdoc />
    public partial class fix_unique_indexes_partial_softdelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_FinancialInstitutions_SwiftBic",
                table: "FinancialInstitutions");

            migrationBuilder.DropIndex(
                name: "UX_FinancialInstitutions_TaxId",
                table: "FinancialInstitutions");

            migrationBuilder.CreateIndex(
                name: "UX_FinancialInstitutions_SwiftBic",
                table: "FinancialInstitutions",
                column: "SwiftBic",
                unique: true,
                filter: "\"SwiftBic\" IS NOT NULL AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "UX_FinancialInstitutions_TaxId",
                table: "FinancialInstitutions",
                columns: new[] { "TaxIdValue", "TaxIdCountryCode" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_FinancialInstitutions_SwiftBic",
                table: "FinancialInstitutions");

            migrationBuilder.DropIndex(
                name: "UX_FinancialInstitutions_TaxId",
                table: "FinancialInstitutions");

            migrationBuilder.CreateIndex(
                name: "UX_FinancialInstitutions_SwiftBic",
                table: "FinancialInstitutions",
                column: "SwiftBic",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_FinancialInstitutions_TaxId",
                table: "FinancialInstitutions",
                columns: new[] { "TaxIdValue", "TaxIdCountryCode" },
                unique: true);
        }
    }
}
