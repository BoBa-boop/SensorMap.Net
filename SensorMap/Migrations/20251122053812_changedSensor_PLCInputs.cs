using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class changedSensor_PLCInputs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "XPoint",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "YPoint",
                table: "SensorAssignments");

            migrationBuilder.RenameColumn(
                name: "TypePLC",
                table: "PLCs",
                newName: "Manufacturer");

            migrationBuilder.AddColumn<byte[]>(
                name: "LocationImage",
                table: "Sensors",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "XPoint",
                table: "Sensors",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "YPoint",
                table: "Sensors",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationImage",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "XPoint",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "YPoint",
                table: "Sensors");

            migrationBuilder.RenameColumn(
                name: "Manufacturer",
                table: "PLCs",
                newName: "TypePLC");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "SensorAssignments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "XPoint",
                table: "SensorAssignments",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "YPoint",
                table: "SensorAssignments",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
