using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRoute.Migrations
{
    /// <inheritdoc />
    public partial class ChangesInCreditListingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreditAmount",
                table: "CreditListings",
                newName: "CreditsListed");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "CreditListings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "CreditListings");

            migrationBuilder.RenameColumn(
                name: "CreditsListed",
                table: "CreditListings",
                newName: "CreditAmount");
        }
    }
}
