using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class AddMapObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SensorAssignments_Mechanisms_MechanismId",
                table: "SensorAssignments");

            migrationBuilder.DropIndex(
                name: "IX_SensorAssignments_MechanismId",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "MechanismId",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "X",
                table: "SensorAssignments");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "SensorAssignments");

            migrationBuilder.CreateTable(
                name: "MapObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    X = table.Column<double>(type: "REAL", nullable: false),
                    Y = table.Column<double>(type: "REAL", nullable: false),
                    Width = table.Column<double>(type: "REAL", nullable: false),
                    Height = table.Column<double>(type: "REAL", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Image = table.Column<byte[]>(type: "BLOB", nullable: true),
                    MechanismId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MapObjects_Mechanisms_MechanismId",
                        column: x => x.MechanismId,
                        principalTable: "Mechanisms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceAssignments_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceAssignments_MapObjects_Id",
                        column: x => x.Id,
                        principalTable: "MapObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAssignments_DeviceId",
                table: "DeviceAssignments",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_MapObjects_MechanismId",
                table: "MapObjects",
                column: "MechanismId");

            migrationBuilder.AddForeignKey(
                name: "FK_SensorAssignments_MapObjects_Id",
                table: "SensorAssignments",
                column: "Id",
                principalTable: "MapObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.Sql(@"INSERT INTO MapObjects (Id, X, Y, Width, Height, Description, Image, MechanismId)
SELECT Id, X, Y, Width, Height, Description, Image, MechanismId
FROM SensorAssignments;
", suppressTransaction: true);
            migrationBuilder.Sql("PRAGMA foreign_keys = 1;",
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SensorAssignments_MapObjects_Id",
                table: "SensorAssignments");

            migrationBuilder.DropTable(
                name: "DeviceAssignments");

            migrationBuilder.DropTable(
                name: "MapObjects");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SensorAssignments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Height",
                table: "SensorAssignments",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
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
                name: "Width",
                table: "SensorAssignments",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

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

            migrationBuilder.CreateIndex(
                name: "IX_SensorAssignments_MechanismId",
                table: "SensorAssignments",
                column: "MechanismId");

            migrationBuilder.AddForeignKey(
                name: "FK_SensorAssignments_Mechanisms_MechanismId",
                table: "SensorAssignments",
                column: "MechanismId",
                principalTable: "Mechanisms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
