using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intec.Banking.FinancialInstitutions.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
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
                    TaxIdValue = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TaxIdCountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    SwiftBic = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<int>(type: "integer", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialInstitutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialInstitutionColombianDetails",
                columns: table => new
                {
                    FinancialInstitutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AchCodeType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AchCountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    SuperFinancialCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialInstitutionColombianDetails", x => x.FinancialInstitutionId);
                    table.ForeignKey(
                        name: "FK_FinancialInstitutionColombianDetails_FinancialInstitutions_~",
                        column: x => x.FinancialInstitutionId,
                        principalTable: "FinancialInstitutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_FinancialInstitutions_TenantId",
                table: "FinancialInstitutions",
                column: "TenantId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialInstitutionColombianDetails");

            migrationBuilder.DropTable(
                name: "FinancialInstitutionLocalCodes");

            migrationBuilder.DropTable(
                name: "FinancialInstitutions");
        }
    }
}
