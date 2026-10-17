using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SensorMap.Model;

namespace SensorMap.Services.Logger
{
    internal static partial class ChangeDescriber
    {
        /// <summary>
        /// Описание назначений датчиков и устройств (наследники MapObject) с указанием сектора и оборудования.
        /// </summary>
        /// <returns>
        /// false, если это не назначение (тогда работает обычное описание);
        /// true, если запись обработана (log может остаться null, если реальных изменений нет).
        /// </returns>
        private static bool TryDescribeMapObject(
            DbContext ctx, EntityEntry entry, MapObject obj, DateTime now, out DbActionLogs? log)
        {
            log = null;

            string title, addedVerb, removedVerb;
            switch (obj)
            {
                case SensorAssignments:
                    (title, addedVerb, removedVerb) = ("Датчик", "назначен", "снят");
                    break;
                case DeviceAssignment:
                    (title, addedVerb, removedVerb) = ("Устройство", "назначено", "снято");
                    break;
                default:
                    return false;
            }

            var entityType = TypeName(obj.GetType());
            var name = NameOf(obj);
            var subject = name.Length > 0 ? $"{title} \"{name}\"" : title;

            var (mechanism, sector) = LocationOf(ctx, obj);
            var place = Place(sector, mechanism);

            switch (entry.State)
            {
                case EntityState.Added:
                    log = NewLog(DbActionKind.Add, entityType, now,
                        place.Length > 0 ? $"{subject} {addedVerb}: {place}" : $"{subject} {addedVerb}");
                    break;

                case EntityState.Deleted:
                    log = NewLog(DbActionKind.Delete, entityType, now,
                        place.Length > 0 ? $"{subject} {removedVerb}: {place}" : $"{subject} {removedVerb}");
                    break;

                case EntityState.Modified:
                    var changes = DescribeChanges(ctx, entry);
                    if (changes.Count > 0)
                    {
                        var where = place.Length > 0 ? $" ({place})" : "";
                        log = NewLog(DbActionKind.Change, entityType, now,
                            $"{subject}{where}: {string.Join("; ", changes)}");
                    }
                    break;
            }

            return true;
        }

        /// <summary>
        /// Оборудование и сектор, к которым относится объект карты.
        /// Сначала берётся из уже заполненных навигационных свойств, иначе одним лёгким запросом.
        /// </summary>
        private static (string Mechanism, string Sector) LocationOf(DbContext ctx, MapObject obj)
        {
            string mechanism = "", sector = "";

            if (obj.Mechanism != null)
            {
                mechanism = NameOf(obj.Mechanism);
                sector = obj.Mechanism.Sector != null ? NameOf(obj.Mechanism.Sector) : "";
            }

            if ((mechanism.Length == 0 || sector.Length == 0)
                && ctx.Entry(obj).Property(nameof(MapObject.MechanismId)).CurrentValue is { } rawId)
            {
                var id = Convert.ToInt32(rawId);

                // Проекция только имён: у оборудования есть схема-изображение, целиком строку грузить не нужно.
                var found = ctx.Set<Mechanism>().AsNoTracking()
                    .Where(m => m.Id == id)
                    .Select(m => new { m.Name, SectorName = m.Sector != null ? m.Sector.Name : null })
                    .FirstOrDefault();

                if (found != null)
                {
                    if (mechanism.Length == 0) mechanism = found.Name ?? "";
                    if (sector.Length == 0) sector = found.SectorName ?? "";
                }
            }

            return (mechanism, sector);
        }

        private static string Place(string sector, string mechanism)
        {
            var parts = new List<string>();
            if (sector.Length > 0) parts.Add($"сектор \"{sector}\"");
            if (mechanism.Length > 0) parts.Add($"оборудование \"{mechanism}\"");
            return string.Join(", ", parts);
        }

        private static DbActionLogs NewLog(DbActionKind kind, string entityType, DateTime now, string description)
            => new()
            {
                Timestamp = now,
                Kind = kind,
                EntityType = entityType,
                Description = description
            };
    }
}
