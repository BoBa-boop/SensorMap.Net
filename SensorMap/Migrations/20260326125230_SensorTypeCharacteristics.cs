using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class SensorTypeCharacteristics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SensorAssignments_Sensors_SensorId",
                table: "SensorAssignments");

            migrationBuilder.CreateTable(
                name: "SensorCharacteristic",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    SensorTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorCharacteristic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorCharacteristic_SensorTypes_SensorTypeId",
                        column: x => x.SensorTypeId,
                        principalTable: "SensorTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SensorCharacteristic_SensorTypeId",
                table: "SensorCharacteristic",
                column: "SensorTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_SensorAssignments_Sensors_SensorId",
                table: "SensorAssignments",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SensorAssignments_Sensors_SensorId",
                table: "SensorAssignments");

            migrationBuilder.DropTable(
                name: "SensorCharacteristic");

            migrationBuilder.RenameColumn(
                name: "Image",
                table: "SensorAssignments",
                newName: "LocationImage");

            migrationBuilder.AddForeignKey(
                name: "FK_SensorAssignments_Sensors_SensorId",
                table: "SensorAssignments",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
