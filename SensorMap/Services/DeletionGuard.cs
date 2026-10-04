using HandyControl.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SensorMap.Logging;
using System.Linq.Expressions;
using System.Reflection;
using EFCore = Microsoft.EntityFrameworkCore.EF;

namespace SensorMap.Services
{
    internal static class DeletionGuard
    {
        private static readonly MethodInfo CountMethod = typeof(DeletionGuard).GetMethod(nameof(CountDependentsAsync), BindingFlags.NonPublic | BindingFlags.Static)!;
        public static async Task<string?> CheckAsync(DbContext dbContext,object entity, CancellationToken ct = default)
        {
            var entry = dbContext.Entry(entity);
            
            var blockers = new List<string>();
            foreach (var nav in entry.Metadata.GetNavigations().Where(n => n.IsCollection))
            {
                var fk = nav.ForeignKey;
                if (fk.DeleteBehavior is not (DeleteBehavior.Restrict or DeleteBehavior.NoAction or DeleteBehavior.ClientNoAction))
                    continue;
                var principalValues = fk.PrincipalKey.Properties.Select(p => entry.Property(p.Name).CurrentValue).ToArray();
                if (principalValues.Any(v => v is null)) continue;
                var dependentType = fk.DeclaringEntityType.ClrType;

                var count = await (Task<int>)CountMethod.MakeGenericMethod(dependentType)
                    .Invoke(null, new object?[] { dbContext,fk,principalValues,ct })!;
                if (count > 0)
                {
                    blockers.Add($"{ChangeDescriber.TypeName(nav.TargetEntityType.ClrType)}({count})");
                } 
            }
            if (blockers.Count == 0)
                return null;
            var type = ChangeDescriber.TypeName(entity.GetType());
            var name = ChangeDescriber.NameOf(entity);
            var title = name.Length > 0 ? $"{type} \"{name}\"" : type;
            Growl.Warning("Нельзя удалить запись. См. логи");
            return $"Нельзя удалить: {title}. С ним связаны записи: {string.Join(",", blockers)}. Сначала удалите или перенесите их.";
        }
        private static Task<int> CountDependentsAsync<TDependent>(DbContext dbContext,IForeignKey fk, object[]? principalValues,CancellationToken ct) where TDependent : class
        {
            var param = Expression.Parameter(typeof(TDependent),"e");
            Expression body = null;
            for (int i = 0; i < fk.Properties.Count; i++)
            {
                var fkProperty = fk.Properties[i];
                var left = Expression.Call(typeof(EFCore), nameof(EFCore.Property), new[] { fkProperty.ClrType }, param, Expression.Constant(fkProperty.Name));
                var right = Expression.Convert(Expression.Constant(principalValues[i]), fkProperty.ClrType);
                var equal = Expression.Equal(left, right);
                body = body == null ? equal : Expression.AndAlso(body, equal);
            }
            var predicate = Expression.Lambda<Func<TDependent, bool>>(body!, param);
            return dbContext.Set<TDependent>().AsNoTracking().CountAsync(predicate,ct);
        }
        public static void RevertPendingChanges(DbContext dbContext)
        {
            foreach (var item in dbContext.ChangeTracker.Entries().ToList())
            {
                switch (item.State)
                {
                    case EntityState.Deleted:
                        item.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        item.State = EntityState.Detached;
                        break;
                    case EntityState.Modified:
                        item.CurrentValues.SetValues(item.OriginalValues);
                        item.State= EntityState.Unchanged;
                        break;
                }
            }
        }
    }
}
