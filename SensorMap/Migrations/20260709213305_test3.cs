using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensorMap.Migrations
{
    /// <inheritdoc />
    public partial class test3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Отключаем проверку внешних ключей
            migrationBuilder.Sql("PRAGMA foreign_keys = 0;",
                suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Включаем обратно
            migrationBuilder.Sql("PRAGMA foreign_keys = 1;",
                suppressTransaction: true);
        }
    }
}
