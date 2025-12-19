using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class addedManufForPLC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SensorAssignments_PLCs_PLCId",
                table: "SensorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorAssignments_Sensors_SensorId",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "LocationImage",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "XPoint",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "YPoint",
                table: "Sensors");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "SensorTypes",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PLCId",
                table: "SensorAssignments",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<byte[]>(
                name: "LocationImage",
                table: "SensorAssignments",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MechanismId",
                table: "SensorAssignments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "X",
                table: "SensorAssignments",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Y",
                table: "SensorAssignments",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

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
                name: "IX_SensorAssignments_MechanismId",
                table: "SensorAssignments",
                column: "MechanismId");

            migrationBuilder.CreateIndex(
                name: "IX_PLCs_PLCManufId",
                table: "PLCs",
                column: "PLCManufId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PLCs_PLC_Manufacturers_PLCManufId",
                table: "PLCs");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorAssignments_Mechanisms_MechanismId",
                table: "SensorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorAssignments_PLCs_PLCId",
                table: "SensorAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorAssignments_Sensors_SensorId",
                table: "SensorAssignments");

            migrationBuilder.DropTable(
                name: "PLC_Manufacturers");

            migrationBuilder.DropIndex(
                name: "IX_SensorAssignments_MechanismId",
                table: "SensorAssignments");

            migrationBuilder.DropIndex(
                name: "IX_PLCs_PLCManufId",
                table: "PLCs");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "SensorTypes");

            migrationBuilder.DropColumn(
                name: "LocationImage",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "MechanismId",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "X",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "PLCManufId",
                table: "PLCs");

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

            migrationBuilder.AlterColumn<int>(
                name: "PLCId",
                table: "SensorAssignments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SensorAssignments_PLCs_PLCId",
                table: "SensorAssignments",
                column: "PLCId",
                principalTable: "PLCs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
