using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SensorMap.Model;

namespace SensorMap.Services.Logger
{
    internal partial class ChangeDescriber
    {
        /// <summary>
        /// Описание вспомогательных файлов с указанием сектора и оборудования.
        /// </summary>
        /// <returns>
        /// false, если это не HelpfulFiles;
        /// true, если запись обработана (log может остаться null, если реальных изменений нет).
        /// </returns>
        private static bool TryDescribeHelpfulFile(
            DbContext ctx, EntityEntry entry, HelpfulFile obj, DateTime now, out DbActionLogs? log)
        {
            log = null;

            string title, addedVerb, removedVerb;
            (title, addedVerb, removedVerb) = ("Файл", "добавлен", "удален");

            var entityType = TypeName(obj.GetType());
            var name = NameOf(obj);
            var subject = name.Length > 0 ? $"{title} \"{name}\"" : title;
            var parent = ParentOf(obj);
            
            switch (entry.State)
            {
                case EntityState.Added:
                    log = NewLog(DbActionKind.Add, entityType, now,
                        $"{TypeName(parent.GetType())} \"{NameOf(parent)}\": добавлен файл \"{name}\"");
                    break;

                case EntityState.Deleted:
                    log = NewLog(DbActionKind.Delete, entityType, now,
                        $"{TypeName(parent.GetType())} \"{NameOf(parent)}\": удалён путь к файлу \"{name}\"");
                    break;

                case EntityState.Modified:
                    var changes = DescribeChanges(ctx, entry);
                    if (changes.Count == 0)
                        break;
                    log = NewLog(DbActionKind.Change,entityType,now,
                        $"{entityType}{subject}: {string.Join("; ", changes)}");
                    break;
            }
            return true;
        }

        private static object? ParentOf(HelpfulFile file)
        {
            object? parent = file.Sensor;
            parent ??= file.Device;
            parent ??= file.Mechanism;
            return parent;
        }


    }
}
