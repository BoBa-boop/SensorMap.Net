using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Services
{
    public class DataBaseEvents
    {// Subject для публикации событий удаления сектора
        public static readonly ISubject<TEntityEvent> EntityDeleted = new Subject<TEntityEvent>();
        public static readonly ISubject<TEntityEvent> EntityUpdated = new Subject<TEntityEvent>();
        public static readonly ISubject<TEntityEvent> EntityCreated = new Subject<TEntityEvent>();

        /// <summary>
        /// Уведомление удаления сущности
        /// </summary>
        public static void RaiseEntityDeleted<T>(T entity) where T : class
        {
            var idProp = entity.GetType().GetProperty("Id");
            if (idProp != null)
            {
                int id = (int)idProp.GetValue(entity)!;
                EntityDeleted.OnNext(new TEntityEvent(id, entity.GetType()));
            }
        }
        /// <summary>
        /// Уведомление создания сущности
        /// </summary>
        public static void RaiseEntityCreated<T>(T entity) where T : class
        {
            var idProp = entity.GetType().GetProperty("Id");
            if (idProp != null)
            {
                int id = (int)idProp.GetValue(entity)!;
                EntityCreated.OnNext(new TEntityEvent(id, entity.GetType()));
            }
        }
        /// <summary>
        /// Уведомление обновления сущности
        /// </summary>
        public static void RaiseEntityUpdated<T>(T entity) where T : class
        {
            var idProp = entity.GetType().GetProperty("Id");
            if (idProp != null)
            {
                int id = (int)idProp.GetValue(entity)!;
                EntityUpdated.OnNext(new TEntityEvent(id, entity.GetType()));
            }
        }
        public struct TEntityEvent
        {
            public int Id { get; }
            public Type EntityType { get; }

            public TEntityEvent(int id, Type entityType)
            {
                Id = id;
                EntityType = entityType;
            }
        }
    }
}
