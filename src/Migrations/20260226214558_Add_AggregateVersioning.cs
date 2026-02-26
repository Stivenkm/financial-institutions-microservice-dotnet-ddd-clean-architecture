using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intec.Banking.FinancialInstitutions.Migrations
{
    /// <inheritdoc />
    public partial class Add_AggregateVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "FinancialInstitutions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "FinancialInstitutions");
        }
    }
}
