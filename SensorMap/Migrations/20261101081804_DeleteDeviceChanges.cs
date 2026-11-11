using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class DeleteDeviceChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MapObjects_Mechanisms_MechanismId",
                table: "MapObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Mechanisms_Devices_DeviceID",
                table: "Mechanisms");

            migrationBuilder.AddForeignKey(
                name: "FK_MapObjects_Mechanisms_MechanismId",
                table: "MapObjects",
                column: "MechanismId",
                principalTable: "Mechanisms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Mechanisms_Devices_DeviceID",
                table: "Mechanisms",
                column: "DeviceID",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MapObjects_Mechanisms_MechanismId",
                table: "MapObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Mechanisms_Devices_DeviceID",
                table: "Mechanisms");

            migrationBuilder.AddForeignKey(
                name: "FK_MapObjects_Mechanisms_MechanismId",
                table: "MapObjects",
                column: "MechanismId",
                principalTable: "Mechanisms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mechanisms_Devices_DeviceID",
                table: "Mechanisms",
                column: "DeviceID",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
