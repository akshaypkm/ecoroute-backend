using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRoute.Migrations
{
    /// <inheritdoc />
    public partial class AddedDimensionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditListings_Companies_SellerCompanyId",
                table: "CreditListings");

            migrationBuilder.DropIndex(
                name: "IX_CreditListings_SellerCompanyId",
                table: "CreditListings");

            migrationBuilder.AddColumn<double>(
                name: "ShipmentHeight",
                table: "Shipments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ShipmentLength",
                table: "Shipments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ShipmentWidth",
                table: "Shipments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OrderHeight",
                table: "Orders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OrderLength",
                table: "Orders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OrderWidth",
                table: "Orders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShipmentHeight",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShipmentLength",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ShipmentWidth",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "OrderHeight",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderLength",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderWidth",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_CreditListings_SellerCompanyId",
                table: "CreditListings",
                column: "SellerCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditListings_Companies_SellerCompanyId",
                table: "CreditListings",
                column: "SellerCompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
