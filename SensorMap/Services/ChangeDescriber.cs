using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace SensorMap.Logging
{
    /// <summary>
    /// Превращает изменения из ChangeTracker в понятные пользователю записи журнала.
    /// Вызывать ДО сохранения (пока состояния Added/Modified/Deleted ещё не сброшены).
    /// </summary>
    internal static class ChangeDescriber
    {
        private static readonly Dictionary<Type, string> TypeNames = new()
        {
            [typeof(Sector)] = "Сектор",
            [typeof(Mechanism)] = "Оборудование",
            [typeof(Sensor)] = "Датчик",
            [typeof(SensorType)] = "Тип датчика",
            [typeof(Device)] = "Устройство",
            [typeof(DeviceType)] = "Тип устройства",
            [typeof(HelpfulFile)] = "Файл",
            [typeof(SensorCharacteristic)] = "Характеристика датчика",
            [typeof(DeviceCharacteristic)] = "Характеристика устройства",
            [typeof(SensorAssignments)] = "Назначение датчика",
            [typeof(DeviceAssignment)] = "Назначение устройства",
            [typeof(MapObject)] = "Объект карты",
        };

        // Русские названия свойств. Приоритетнее: атрибут [Display(Name = "...")] на самом свойстве модели.
        // Дополните под свои модели.
        private static readonly Dictionary<string, string> PropertyNames = new()
        {
            ["Name"] = "Название",
            ["Title"] = "Заголовок",
            ["Description"] = "Описание",
            ["NameFile"] = "Имя файла",
            ["X"] = "Позиция X",
            ["Y"] = "Позиция Y",
            ["Width"] = "Ширина",
            ["Height"] = "Высота",
            ["Value"] = "Значение",
            ["Unit"] = "Единица измерения",
        };

        // Свойства, из которых берём "имя" объекта для записи в журнале
        private static readonly string[] NameProperties = { "Name", "Title", "Description", "NameFile" };

        private const int MaxValueLength = 60;

        public static List<DbActionLogs> Build(DbContext ctx)
        {
            var now = DateTime.Now;
            var result = new List<DbActionLogs>();

            // ChangeTracker.Entries() сам вызывает DetectChanges()
            var entries = ctx.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                var log = Describe(ctx, entry, now);
                if (log != null)
                    result.Add(log);
            }

            return result;
        }

        private static DbActionLogs? Describe(DbContext ctx, EntityEntry entry, DateTime now)
        {
            var typeName = TypeName(entry.Metadata.ClrType);
            var name = NameOf(entry.Entity);
            var quotedName = name.Length > 0 ? $" \"{name}\"" : "";

            DbActionKind kind;
            string description;

            switch (entry.State)
            {
                case EntityState.Added:
                    kind = DbActionKind.Add;
                    description = entry.Entity is HelpfulFile addedFile && ParentOf(addedFile) is { } addedParent
                        ? $"{TypeName(addedParent.GetType())} \"{NameOf(addedParent)}\": добавлен файл \"{name}\""
                        : $"{typeName}{quotedName} — добавление";
                    break;

                case EntityState.Deleted:
                    kind = DbActionKind.Delete;
                    description = entry.Entity is HelpfulFile deletedFile && ParentOf(deletedFile) is { } deletedParent
                        ? $"{TypeName(deletedParent.GetType())} \"{NameOf(deletedParent)}\": удалён файл \"{name}\""
                        : $"{typeName}{quotedName} — удаление";
                    break;

                case EntityState.Modified:
                    var changes = DescribeChanges(ctx, entry);
                    if (changes.Count == 0)
                        return null; // формально Modified, но значения не поменялись
                    kind = DbActionKind.Change;
                    description = $"{typeName}{quotedName}: {string.Join("; ", changes)}";
                    break;

                default:
                    return null;
            }

            return new DbActionLogs
            {
                Timestamp = now,
                Kind = kind,
                EntityType = typeName,
                Description = description
            };
        }

        private static List<string> DescribeChanges(DbContext ctx, EntityEntry entry)
        {
            var clrType = entry.Metadata.ClrType;
            var list = new List<string>();

            foreach (var p in entry.Properties)
            {
                if (!p.IsModified || p.Metadata.IsPrimaryKey())
                    continue;

                if (SameValue(p.OriginalValue, p.CurrentValue))
                    continue;

                // Внешний ключ: показываем не число, а имя связанного объекта
                var fk = p.Metadata.GetContainingForeignKeys().FirstOrDefault();
                if (fk != null)
                {
                    var principal = fk.PrincipalEntityType.ClrType;
                    var oldName = ResolveReference(ctx, principal, p.OriginalValue);
                    var newName = ResolveReference(ctx, principal, p.CurrentValue);
                    list.Add($"{TypeName(principal)} \"{oldName}\" → \"{newName}\"");
                    continue;
                }

                var label = PropertyName(clrType, p.Metadata.Name);
                list.Add($"{label} \"{Format(p.OriginalValue)}\" → \"{Format(p.CurrentValue)}\"");
            }

            return list;
        }

        private static string ResolveReference(DbContext ctx, Type principalType, object? id)
        {
            if (id is null)
                return "—";

            try
            {
                var target = ctx.Find(principalType, id);
                var name = target is null ? "" : NameOf(target);
                return name.Length > 0 ? name : $"#{id}";
            }
            catch
            {
                return $"#{id}";
            }
        }

        private static object? ParentOf(HelpfulFile file)
        {
            object? parent = file.Sensor;
            parent ??= file.Device;
            parent ??= file.Mechanism;
            return parent;
        }

        internal static string NameOf(object entity)
        {
            var direct = ReadName(entity);
            if (direct.Length > 0)
                return direct;

            // У назначений (TPT-наследники MapObject) своего имени нет: берём имя датчика/устройства
            object? related = entity switch
            {
                SensorAssignments a => a.Sensor,
                DeviceAssignment d => d.Device,
                _ => null
            };

            return related is null ? "" : ReadName(related);
        }

        private static string ReadName(object entity)
        {
            var type = entity.GetType();
            foreach (var propName in NameProperties)
            {
                var value = type.GetProperty(propName)?.GetValue(entity) as string;
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
            return "";
        }

        internal static string TypeName(Type type)
        {
            // Идём вверх по иерархии: покрывает и EF-прокси, и наследников
            for (var t = type; t != null; t = t.BaseType)
            {
                if (TypeNames.TryGetValue(t, out var name))
                    return name;
            }
            return type.Name;
        }

        private static string PropertyName(Type clrType, string propertyName)
        {
            var display = clrType.GetProperty(propertyName)?.GetCustomAttribute<DisplayAttribute>()?.GetName();
            if (!string.IsNullOrEmpty(display))
                return display;

            return PropertyNames.TryGetValue(propertyName, out var ru) ? ru : propertyName;
        }

        private static bool SameValue(object? a, object? b)
        {
            if (a is byte[] x && b is byte[] y)
                return x.AsSpan().SequenceEqual(y);
            return Equals(a, b);
        }

        private static string Format(object? value) => value switch
        {
            null => "—",
            string s => s.Length == 0 ? "(пусто)" : Truncate(s),
            bool b => b ? "Да" : "Нет",
            DateTime dt => dt.ToString("dd.MM.yyyy HH:mm", CultureInfo.CurrentCulture),
            byte[] bytes => $"{bytes.Length} байт",
            Enum e => e.ToString(),
            IFormattable f => f.ToString(null, CultureInfo.CurrentCulture),
            _ => Truncate(value.ToString() ?? "")
        };

        private static string Truncate(string s)
            => s.Length <= MaxValueLength ? s : s[..MaxValueLength] + "…";
    }
}
