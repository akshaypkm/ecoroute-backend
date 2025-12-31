using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRoute.Migrations
{
    /// <inheritdoc />
    public partial class RemovedMontlyCreditsListCreatedRemainingCredits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MonthlyEmissionsCap",
                table: "Companies",
                newName: "RemainingCredits");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RemainingCredits",
                table: "Companies",
                newName: "MonthlyEmissionsCap");
        }
    }
}
