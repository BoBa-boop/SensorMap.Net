using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class DeleteImageFromSensorType_DeviceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "SensorTypes");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "DeviceTypes");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "SensorTypes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "SensorTypes");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "SensorTypes",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "DeviceTypes",
                type: "BLOB",
                nullable: true);
        }
    }
}
