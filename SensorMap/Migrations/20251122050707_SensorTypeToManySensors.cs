using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class SensorTypeToManySensors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sensors_SensorTypeID",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Sensors");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_SensorTypeID",
                table: "Sensors",
                column: "SensorTypeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sensors_SensorTypeID",
                table: "Sensors");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Sensors",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_SensorTypeID",
                table: "Sensors",
                column: "SensorTypeID",
                unique: true);
        }
    }
}
