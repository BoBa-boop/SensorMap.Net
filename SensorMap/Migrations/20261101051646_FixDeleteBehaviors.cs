using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class FixDeleteBehaviors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceTypes_DeviceTypeId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_SensorTypes_SensorTypeID",
                table: "Sensors");

            migrationBuilder.AlterColumn<byte[]>(
                name: "ImageFile",
                table: "HelpfulFiles",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceTypes_DeviceTypeId",
                table: "Devices",
                column: "DeviceTypeId",
                principalTable: "DeviceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_SensorTypes_SensorTypeID",
                table: "Sensors",
                column: "SensorTypeID",
                principalTable: "SensorTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceTypes_DeviceTypeId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_SensorTypes_SensorTypeID",
                table: "Sensors");

            migrationBuilder.AlterColumn<byte[]>(
                name: "ImageFile",
                table: "HelpfulFiles",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceTypes_DeviceTypeId",
                table: "Devices",
                column: "DeviceTypeId",
                principalTable: "DeviceTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_SensorTypes_SensorTypeID",
                table: "Sensors",
                column: "SensorTypeID",
                principalTable: "SensorTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
