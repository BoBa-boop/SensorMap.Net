using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class ImageNotNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Image",
                table: "Sensors",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "VARBINARY(MAX)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Image",
                table: "Sectors",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "VARBINARY(MAX)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Image",
                table: "PLCs",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "VARBINARY(MAX)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Image",
                table: "Mechanisms",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "VARBINARY(MAX)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Image",
                table: "Sensors",
                type: "VARBINARY(MAX)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Image",
                table: "Sectors",
                type: "VARBINARY(MAX)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Image",
                table: "PLCs",
                type: "VARBINARY(MAX)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Image",
                table: "Mechanisms",
                type: "VARBINARY(MAX)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);
        }
    }
}
