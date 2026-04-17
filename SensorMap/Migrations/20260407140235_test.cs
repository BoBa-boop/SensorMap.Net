using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mechanisms_PLCs_PLCID",
                table: "Mechanisms");

            migrationBuilder.RenameTable(
                name: "PLCs",
                newName: "Devices");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "Mechanisms");

            migrationBuilder.RenameColumn(
                name: "PLCID",
                table: "Mechanisms",
                newName: "DeviceID");

            migrationBuilder.RenameIndex(
                name: "IX_Mechanisms_PLCID",
                table: "Mechanisms",
                newName: "IX_Mechanisms_DeviceID");

            migrationBuilder.CreateTable(
                name: "DeviceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Image = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceCharacteristic",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceCharacteristic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceCharacteristic_DeviceTypes_DeviceTypeId",
                        column: x => x.DeviceTypeId,
                        principalTable: "DeviceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<int>(
                name: "MasterDeviceID",
                type:"INTEGER",
                nullable: true,
                table: "Devices");

            migrationBuilder.AddColumn<int>(
                name: "DeviceTypeID",
                type: "INTEGER",
                nullable: true,
                table: "Devices");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCharacteristic_DeviceTypeId",
                table: "DeviceCharacteristic",
                column: "DeviceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceTypeID",
                table: "Devices",
                column: "DeviceTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_MasterDeviceID",
                table: "Devices",
                column: "MasterDeviceID");

            migrationBuilder.AddForeignKey(
                name: "FK_Mechanisms_Devices_DeviceID",
                table: "Mechanisms",
                column: "DeviceID",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mechanisms_Devices_DeviceID",
                table: "Mechanisms");

            migrationBuilder.DropTable(
                name: "DeviceCharacteristic");

            migrationBuilder.DropTable(
                name: "DeviceTypes");

            migrationBuilder.RenameColumn(
                name: "DeviceID",
                table: "Mechanisms",
                newName: "PLCID");

            migrationBuilder.RenameIndex(
                name: "IX_Mechanisms_DeviceID",
                table: "Mechanisms",
                newName: "IX_Mechanisms_PLCID");

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "Mechanisms",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropColumn(
                name: "MasterDeviceID",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "DeviceTypeID",
                table: "Devices");
            migrationBuilder.RenameTable(
                name: "Devices",
                newName: "PLCs");

            migrationBuilder.AddForeignKey(
                name: "FK_Mechanisms_PLCs_PLCID",
                table: "Mechanisms",
                column: "PLCID",
                principalTable: "PLCs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
