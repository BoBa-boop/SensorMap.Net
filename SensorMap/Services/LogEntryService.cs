using SensorMap.Interfaces;
using SensorMap.Logging;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Application = System.Windows.Application;

namespace SensorMap.Services
{
    public class LogEntryService : ILogEntryService, IDisposable
    {
        private const int BatchSize = 100;

        // Путь должен совпадать с fileName у target "dbLog" в nlog.config
        private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "LogDb.txt");

        private int _loadedFromIndex;

        public ObservableCollection<DbActionLogs> Logs { get; } = new();

        public bool CanLoadMore => _loadedFromIndex > 0;

        public LogEntryService()
        {
            DbLog.Written += AddLogEntry;
            LoadFromFile();
        }

        public void LoadFromFile(string? path = null)
        {
            path ??= _filePath;
            if (!File.Exists(path)) return;

            var totalLines = ReadLinesShared(path).Count();
            _loadedFromIndex = Math.Max(0, totalLines - BatchSize);

            foreach (var log in ReadRange(path, _loadedFromIndex, totalLines))
                Logs.Add(log);
        }

        public void LoadMore()
        {
            if (!CanLoadMore) return;

            var nextStart = Math.Max(0, _loadedFromIndex - BatchSize);
            var older = ReadRange(_filePath, nextStart, _loadedFromIndex);

            for (int i = older.Count - 1; i >= 0; i--)
                Logs.Insert(0, older[i]);

            _loadedFromIndex = nextStart;
        }

        private static List<DbActionLogs> ReadRange(string path, int start, int end)
        {
            var result = new List<DbActionLogs>();
            foreach (var line in ReadLinesShared(path).Skip(start).Take(end - start))
            {
                var log = ParseLine(line);
                if (log != null) result.Add(log);
            }
            return result;
        }

        /// <summary>
        /// Формат строки NLog: "{longdate}|{level}|{logger}|{message}".
        /// Новые записи: message = JSON. Старые записи: message = обычный текст.
        /// </summary>
        private static DbActionLogs? ParseLine(string line)
        {
            var parts = line.Split('|', 4);
            if (parts.Length < 4) return null;

            var message = parts[3].TrimStart();

            if (message.StartsWith('{'))
            {
                var parsed = DbLog.TryParse(message);
                if (parsed != null) return parsed;
            }

            // Старый формат
            if (!DateTime.TryParse(parts[0], out var ts)) return null;

            return new DbActionLogs
            {
                Timestamp = ts,
                Kind = parts[1].Trim().Equals("ERROR", StringComparison.OrdinalIgnoreCase)
                    ? DbActionKind.Error
                    : GuessKind(message),
                Description = message
            };
        }

        private static DbActionKind GuessKind(string text)
        {
            if (text.Contains("Добавление", StringComparison.OrdinalIgnoreCase)) return DbActionKind.Add;
            if (text.Contains("Удаление", StringComparison.OrdinalIgnoreCase)) return DbActionKind.Delete;
            return DbActionKind.Change;
        }

        /// <summary>
        /// Чтение без блокировки: NLog может писать в этот файл в тот же момент.
        /// </summary>
        private static IEnumerable<string> ReadLinesShared(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            string? line;
            while ((line = reader.ReadLine()) != null)
                yield return line;
        }

        private void AddLogEntry(DbActionLogs log)
        {
            var dispatcher = Application.Current?.Dispatcher;

            if (dispatcher == null || dispatcher.CheckAccess())
                Logs.Add(log);
            else
                dispatcher.BeginInvoke(new Action(() => Logs.Add(log))); // не Invoke: исключаем взаимную блокировку с UI
        }

        public void Dispose()
        {
            DbLog.Written -= AddLogEntry;
        }
    }
}
