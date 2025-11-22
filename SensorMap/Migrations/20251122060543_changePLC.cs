using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class changePLC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PLCs_Mechanisms_MechId",
                table: "PLCs");

            migrationBuilder.DropIndex(
                name: "IX_PLCs_MechId",
                table: "PLCs");

            migrationBuilder.DropColumn(
                name: "MechId",
                table: "PLCs");

            migrationBuilder.AlterColumn<string>(
                name: "Manufacturer",
                table: "PLCs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "IP",
                table: "PLCs",
                type: "TEXT",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 15);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "PLCs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PLCID",
                table: "Mechanisms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Mechanisms_PLCID",
                table: "Mechanisms",
                column: "PLCID");

            migrationBuilder.AddForeignKey(
                name: "FK_Mechanisms_PLCs_PLCID",
                table: "Mechanisms",
                column: "PLCID",
                principalTable: "PLCs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mechanisms_PLCs_PLCID",
                table: "Mechanisms");

            migrationBuilder.DropIndex(
                name: "IX_Mechanisms_PLCID",
                table: "Mechanisms");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "PLCs");

            migrationBuilder.DropColumn(
                name: "PLCID",
                table: "Mechanisms");

            migrationBuilder.AlterColumn<string>(
                name: "Manufacturer",
                table: "PLCs",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IP",
                table: "PLCs",
                type: "TEXT",
                maxLength: 15,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 15,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MechId",
                table: "PLCs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PLCs_MechId",
                table: "PLCs",
                column: "MechId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PLCs_Mechanisms_MechId",
                table: "PLCs",
                column: "MechId",
                principalTable: "Mechanisms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
