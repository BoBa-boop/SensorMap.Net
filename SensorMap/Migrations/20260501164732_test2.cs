using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class test2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HelpfulFiles_Mechanisms_MechanismId",
                table: "HelpfulFiles");

            migrationBuilder.AddForeignKey(
                name: "FK_HelpfulFiles_Mechanisms_MechanismId",
                table: "HelpfulFiles",
                column: "MechanismId",
                principalTable: "Mechanisms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HelpfulFiles_Mechanisms_MechanismId",
                table: "HelpfulFiles");

            migrationBuilder.AddForeignKey(
                name: "FK_HelpfulFiles_Mechanisms_MechanismId",
                table: "HelpfulFiles",
                column: "MechanismId",
                principalTable: "Mechanisms",
                principalColumn: "Id");
        }
    }
}
