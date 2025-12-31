using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRoute.Migrations
{
    /// <inheritdoc />
    public partial class AddedTruckType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TruckTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TruckName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GVWKg = table.Column<double>(type: "float", nullable: false),
                    MaxPayloadKg = table.Column<double>(type: "float", nullable: false),
                    CargoLengthMeters = table.Column<double>(type: "float", nullable: false),
                    CargoWidthMeters = table.Column<double>(type: "float", nullable: false),
                    CargoHeightMeters = table.Column<double>(type: "float", nullable: false),
                    KerbWeight = table.Column<double>(type: "float", nullable: false),
                    EngineEfficiency = table.Column<double>(type: "float", nullable: false),
                    DragCoefficient = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckTypes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TruckTypes");
        }
    }
}
