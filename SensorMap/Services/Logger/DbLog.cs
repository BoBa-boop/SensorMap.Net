using SensorMap.Model;
using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SensorMap.Logging
{
    /// <summary>
    /// Единая точка записи журнала действий с БД.
    /// Пишет одну JSON-строку через NLog и уведомляет UI (LogEntryService).
    /// </summary>
    internal static class DbLog
    {
        // Отдельный именованный логгер, чтобы для него можно было задать свой файл в nlog.config
        private static readonly NLog.Logger Nlog = NLog.LogManager.GetLogger("DbLog");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            // Чтобы кириллица в файле была читаемой, а не \u041F...
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = false // одна запись = одна строка
        };

        public static event Action<DbActionLogs>? Written;

        public static void Write(DbActionLogs log)
        {
            try
            {
                var json = JsonSerializer.Serialize(log, JsonOptions);

                // "{Json}" + аргумент: фигурные скобки JSON не будут приняты за шаблон NLog
                if (log.Kind == DbActionKind.Error)
                    Nlog.Error("{Json}", json);
                else
                    Nlog.Info("{Json}", json);
            }
            catch
            {
                // журнал не должен ломать приложение
            }

            Written?.Invoke(log);
        }

        public static DbActionLogs? TryParse(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<DbActionLogs>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }
    }
}
