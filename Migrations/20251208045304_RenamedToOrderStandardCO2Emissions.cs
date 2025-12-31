using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRoute.Migrations
{
    /// <inheritdoc />
    public partial class RenamedToOrderStandardCO2Emissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BaselineCO2Emissions",
                table: "Orders",
                newName: "OrderStandardCO2Emissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderStandardCO2Emissions",
                table: "Orders",
                newName: "BaselineCO2Emissions");
        }
    }
}
