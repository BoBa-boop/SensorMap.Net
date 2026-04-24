using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class addeHelpfulfilesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HelpfulFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NameFile = table.Column<string>(type: "TEXT", nullable: false),
                    ImageFile = table.Column<byte[]>(type: "BLOB", nullable: false),
                    SensorId = table.Column<int>(type: "INTEGER", nullable: true),
                    DeviceId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpfulFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HelpfulFiles_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HelpfulFiles_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HelpfulFiles_DeviceId",
                table: "HelpfulFiles",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpfulFiles_SensorId",
                table: "HelpfulFiles",
                column: "SensorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HelpfulFiles");
        }
    }
}
