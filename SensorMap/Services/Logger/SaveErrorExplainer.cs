using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;

namespace SensorMap.Logging
{
    /// <summary>
    /// Переводит исключения EF Core / SQLite в короткое понятное объяснение причины.
    /// Полный текст исключения при этом сохраняется отдельно (DbActionLogs.Technical).
    /// </summary>
    internal static class SaveErrorExplainer
    {
        public static string Explain(Exception ex) => ex switch
        {
            DbUpdateConcurrencyException
                => "данные уже были изменены или удалены другим процессом",

            DbUpdateException { InnerException: SqliteException sqlite }
                => ExplainSqlite(sqlite),

            SqliteException sqlite
                => ExplainSqlite(sqlite),

            DbUpdateException dbu
                => dbu.InnerException?.Message ?? dbu.Message,

            _ => ex.Message
        };

        private static string ExplainSqlite(SqliteException e) => e.SqliteErrorCode switch
        {
            5 or 6 => "база данных занята другим процессом (database is locked)",
            8 => "база данных доступна только для чтения",
            10 => "ошибка чтения или записи файла базы данных",
            11 => "файл базы данных повреждён",
            13 => "недостаточно места на диске",
            14 => "не удаётся открыть файл базы данных",
            19 => ExplainConstraint(e.SqliteExtendedErrorCode),
            _ => e.Message
        };

        private static string ExplainConstraint(int extendedCode) => extendedCode switch
        {
            787 => "нельзя удалить или изменить объект: на него ссылаются другие записи",
            1555 or 2067 => "запись с таким значением уже существует",
            1299 => "не заполнено обязательное поле",
            275 => "значение не проходит проверку (CHECK)",
            _ => "нарушено ограничение целостности базы данных"
        };
    }
}
