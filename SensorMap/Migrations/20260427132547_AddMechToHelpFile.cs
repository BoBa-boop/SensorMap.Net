using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class AddMechToHelpFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MechanismId",
                table: "HelpfulFiles",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HelpfulFiles_MechanismId",
                table: "HelpfulFiles",
                column: "MechanismId");

            migrationBuilder.AddForeignKey(
                name: "FK_HelpfulFiles_Mechanisms_MechanismId",
                table: "HelpfulFiles",
                column: "MechanismId",
                principalTable: "Mechanisms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HelpfulFiles_Mechanisms_MechanismId",
                table: "HelpfulFiles");

            migrationBuilder.DropIndex(
                name: "IX_HelpfulFiles_MechanismId",
                table: "HelpfulFiles");

            migrationBuilder.DropColumn(
                name: "MechanismId",
                table: "HelpfulFiles");
        }
    }
}
