using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class optimizationContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mechanisms_PLCs_PLCID",
                table: "Mechanisms");

            migrationBuilder.DropForeignKey(
                name: "FK_Mechanisms_Sectors_SectorID",
                table: "Mechanisms");

            migrationBuilder.DropForeignKey(
                name: "FK_PLCs_PLC_Manufacturers_PLCManufId",
                table: "PLCs");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorAssignments_Mechanisms_MechanismId",
                table: "SensorAssignments");

            migrationBuilder.DropTable(
                name: "PLC_Manufacturers");

            migrationBuilder.DropIndex(
                name: "IX_PLCs_PLCManufId",
                table: "PLCs");

            migrationBuilder.DropColumn(
                name: "PLCManufId",
                table: "PLCs");

            migrationBuilder.AddForeignKey(
                name: "FK_Mechanisms_PLCs_PLCID",
                table: "Mechanisms",
                column: "PLCID",
                principalTable: "PLCs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Mechanisms_Sectors_SectorID",
                table: "Mechanisms",
                column: "SectorID",
                principalTable: "Sectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SensorAssignments_Mechanisms_MechanismId",
                table: "SensorAssignments",
                column: "MechanismId",
                principalTable: "Mechanisms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mechanisms_PLCs_PLCID",
                table: "Mechanisms");

            migrationBuilder.DropForeignKey(
                name: "FK_Mechanisms_Sectors_SectorID",
                table: "Mechanisms");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorAssignments_Mechanisms_MechanismId",
                table: "SensorAssignments");

            migrationBuilder.AddColumn<int>(
                name: "PLCManufId",
                table: "PLCs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PLC_Manufacturers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 155, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PLC_Manufacturers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PLCs_PLCManufId",
                table: "PLCs",
                column: "PLCManufId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mechanisms_PLCs_PLCID",
                table: "Mechanisms",
                column: "PLCID",
                principalTable: "PLCs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mechanisms_Sectors_SectorID",
                table: "Mechanisms",
                column: "SectorID",
                principalTable: "Sectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PLCs_PLC_Manufacturers_PLCManufId",
                table: "PLCs",
                column: "PLCManufId",
                principalTable: "PLC_Manufacturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SensorAssignments_Mechanisms_MechanismId",
                table: "SensorAssignments",
                column: "MechanismId",
                principalTable: "Mechanisms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
