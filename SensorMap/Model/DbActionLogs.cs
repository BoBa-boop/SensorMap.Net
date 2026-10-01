using System;
using System.Text.Json.Serialization;

namespace SensorMap.Model
{
    public enum DbActionKind
    {
        Add,
        Change,
        Delete,
        Error
    }

    /// <summary>
    /// Одна запись журнала. В файл пишется как одна JSON-строка.
    /// </summary>
    public class DbActionLogs
    {
        public DateTime Timestamp { get; set; }

        public DbActionKind Kind { get; set; }

        /// <summary>Тип объекта по-русски: "Датчик", "Сектор"... (пусто для ошибок).</summary>
        public string EntityType { get; set; } = "";

        /// <summary>Готовый текст для показа пользователю.</summary>
        public string Description { get; set; } = "";

        /// <summary>Технические детали (полный текст исключения). Только для ошибок.</summary>
        public string? Technical { get; set; }

        [JsonIgnore]
        public string KindText => Kind switch
        {
            DbActionKind.Add => "Добавление",
            DbActionKind.Change => "Изменение",
            DbActionKind.Delete => "Удаление",
            DbActionKind.Error => "Ошибка",
            _ => ""
        };
    }
}
