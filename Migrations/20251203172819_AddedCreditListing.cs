using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRoute.Migrations
{
    /// <inheritdoc />
    public partial class AddedCreditListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyCredits",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyEmissionBudget",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CreditListings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerCompanyId = table.Column<int>(type: "int", nullable: false),
                    CreditAmount = table.Column<double>(type: "float", nullable: false),
                    PricePerCredit = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ListedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditListings_Companies_SellerCompanyId",
                        column: x => x.SellerCompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditListings_SellerCompanyId",
                table: "CreditListings",
                column: "SellerCompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditListings");

            migrationBuilder.DropColumn(
                name: "CompanyCredits",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CompanyEmissionBudget",
                table: "Companies");
        }
    }
}
