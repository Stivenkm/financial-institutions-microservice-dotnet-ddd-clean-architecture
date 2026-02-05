using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intec.Banking.FinancialInstitutions.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialInstitutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfficialName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TradeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    SwiftBic = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    AchCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AchCodeType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AchCountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    SuperFinancialCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TaxIdValue = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TaxIdCountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialInstitutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialInstitutionLocalCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CodeType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    FinancialInstitutionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialInstitutionLocalCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialInstitutionLocalCodes_FinancialInstitutions_Financ~",
                        column: x => x.FinancialInstitutionId,
                        principalTable: "FinancialInstitutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialInstitutionLocalCodes_FinancialInstitutionId",
                table: "FinancialInstitutionLocalCodes",
                column: "FinancialInstitutionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialInstitutionLocalCodes");

            migrationBuilder.DropTable(
                name: "FinancialInstitutions");
        }
    }
}
