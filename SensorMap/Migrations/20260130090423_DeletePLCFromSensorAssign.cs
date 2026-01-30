using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class DeletePLCFromSensorAssign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mechanisms_PLCs_PLCID",
                table: "Mechanisms");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorAssignments_PLCs_PLCId",
                table: "SensorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorAssignments_Sensors_SensorId",
                table: "SensorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_SensorTypes_SensorTypeID",
                table: "Sensors");

            migrationBuilder.DropIndex(
                name: "IX_SensorAssignments_PLCId",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "PLCId",
                table: "SensorAssignments");

            migrationBuilder.AddForeignKey(
                name: "FK_Mechanisms_PLCs_PLCID",
                table: "Mechanisms",
                column: "PLCID",
                principalTable: "PLCs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SensorAssignments_Sensors_SensorId",
                table: "SensorAssignments",
                column: "SensorId",
                principalTable: "Sensors",
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
                name: "FK_Mechanisms_PLCs_PLCID",
                table: "Mechanisms");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorAssignments_Sensors_SensorId",
                table: "SensorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_SensorTypes_SensorTypeID",
                table: "Sensors");

            migrationBuilder.AddColumn<int>(
                name: "PLCId",
                table: "SensorAssignments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensorAssignments_PLCId",
                table: "SensorAssignments",
                column: "PLCId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mechanisms_PLCs_PLCID",
                table: "Mechanisms",
                column: "PLCID",
                principalTable: "PLCs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SensorAssignments_PLCs_PLCId",
                table: "SensorAssignments",
                column: "PLCId",
                principalTable: "PLCs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SensorAssignments_Sensors_SensorId",
                table: "SensorAssignments",
                column: "SensorId",
                principalTable: "Sensors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
