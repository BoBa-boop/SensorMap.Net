using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SensorMap.Logging
{
    /// <summary>
    /// Пишет журнал действий с БД:
    ///  - до сохранения собирает описание изменений (пока ChangeTracker ещё их помнит);
    ///  - после успешного сохранения записывает их в журнал;
    ///  - при ошибке записывает понятную причину + полный текст исключения.
    /// Работает и для SaveChanges(), и для SaveChangesAsync().
    /// </summary>
    public sealed class AuditInterceptor : SaveChangesInterceptor
    {
        private sealed class State
        {
            public List<DbActionLogs> Logs { get; set; } = new();
        }

        // Один интерсептор обслуживает все контексты, поэтому состояние хранится по контексту
        private readonly ConditionalWeakTable<DbContext, State> _states = new();

        // ---------- До сохранения ----------

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            Prepare(eventData.Context);
            return result;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Prepare(eventData.Context);
            return new ValueTask<InterceptionResult<int>>(result);
        }

        // ---------- Успех ----------

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            Complete(eventData.Context);
            return result;
        }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            Complete(eventData.Context);
            return new ValueTask<int>(result);
        }

        // ---------- Ошибка ----------

        public override void SaveChangesFailed(DbContextErrorEventData eventData)
            => Fail(eventData.Context, eventData.Exception);

        public override Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            Fail(eventData.Context, eventData.Exception);
            return Task.CompletedTask;
        }

        // ---------- Реализация ----------

        private void Prepare(DbContext? context)
        {
            if (context is null) return;

            try
            {
                _states.GetOrCreateValue(context).Logs = ChangeDescriber.Build(context);
            }
            catch (Exception ex)
            {
                // Сбой в построении журнала не должен мешать самому сохранению
                System.Diagnostics.Debug.WriteLine($"[AuditInterceptor.Prepare] {ex}");
            }
        }

        private void Complete(DbContext? context)
        {
            if (context is null || !_states.TryGetValue(context, out var state))
                return;

            foreach (var log in state.Logs)
                DbLog.Write(log);

            _states.Remove(context);
        }

        private void Fail(DbContext? context, Exception ex)
        {
            List<DbActionLogs> pending = new();
            if (context != null && _states.TryGetValue(context, out var state))
            {
                pending = state.Logs;
                _states.Remove(context);
            }

            var reason = SaveErrorExplainer.Explain(ex);

            var description = $"Не удалось сохранить изменения: {reason}.";
            if (pending.Count > 0)
            {
                var preview = string.Join("; ", pending.Take(3).Select(p => p.Description));
                var more = pending.Count > 3 ? $" и ещё {pending.Count - 3}" : "";
                description += $" Не сохранено: {preview}{more}.";
            }

            DbLog.Write(new DbActionLogs
            {
                Timestamp = DateTime.Now,
                Kind = DbActionKind.Error,
                Description = description,
                Technical = ex.ToString()
            });
        }
    }
}
