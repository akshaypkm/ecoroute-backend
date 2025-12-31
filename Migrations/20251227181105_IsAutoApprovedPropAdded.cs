using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRoute.Migrations
{
    /// <inheritdoc />
    public partial class IsAutoApprovedPropAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAutoApproved",
                table: "Orders",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAutoApproved",
                table: "Orders");
        }
    }
}
