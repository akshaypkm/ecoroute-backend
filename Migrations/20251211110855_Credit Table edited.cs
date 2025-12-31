using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRoute.Migrations
{
    /// <inheritdoc />
    public partial class CreditTableedited : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LatestDate",
                table: "Credits");

            migrationBuilder.RenameColumn(
                name: "CreditMarketPrice",
                table: "Credits",
                newName: "SectorCredits");

            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "Credits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sector",
                table: "Credits");

            migrationBuilder.RenameColumn(
                name: "SectorCredits",
                table: "Credits",
                newName: "CreditMarketPrice");

            migrationBuilder.AddColumn<DateTime>(
                name: "LatestDate",
                table: "Credits",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
