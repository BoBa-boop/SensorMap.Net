using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace SensorMap.Services
{
    public class LogEntryService : ILogEntryService, IDisposable
    {
        private const int BatchSize = 100;
        private readonly string _filePath;
        private int _loadedFromIndex;

        public ObservableCollection<DbActionLogs> Logs { get; } = new();
        public bool CanLoadMore => _loadedFromIndex > 0;

        public LogEntryService()
        {
            _filePath = "LogDb.txt";
            AppDBContext.OnLogEntry += AddLogEntry;
            LoadFromFile();
        }

        public void LoadFromFile(string path = "LogDb.txt")
        {
            if (!File.Exists(path)) return;
            var totalLines = File.ReadLines(path).Count();
            _loadedFromIndex = Math.Max(0, totalLines - BatchSize);
            foreach (var log in ReadLines(path, _loadedFromIndex, totalLines))
            {
                Logs.Add(log);
            }
        }

        public void LoadMore()
        {
            if (!CanLoadMore) return;
            var nextStart = Math.Max(0, _loadedFromIndex - BatchSize);
            var newLogs = ReadLines(_filePath, nextStart, _loadedFromIndex);
            for (int i = newLogs.Count - 1; i >= 0; i--)
            {
                Logs.Insert(0, newLogs[i]);
            }
            _loadedFromIndex = nextStart;
        }

        private static List<DbActionLogs> ReadLines(string path, int start, int end)
        {
            var result = new List<DbActionLogs>();
            foreach (var line in File.ReadLines(path).Skip(start).Take(end - start))
            {
                var log = ParseLine(line);
                if (log != null) result.Add(log);
            }
            return result;
        }

        private static DbActionLogs? ParseLine(string line)
        {
            var parts = line.Split('|', 4);
            if (parts.Length < 4) return null;
            if (!DateTime.TryParse(parts[0], out var ts)) return null;
            return new DbActionLogs { Timestamp = ts, Description = parts[3] };
        }

        private void AddLogEntry(DbActionLogs log)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null)
            {
                dispatcher.Invoke(() => Logs.Add(log));
            }
            else
            {
                Logs.Add(log);
            }
        }

        public void Dispose()
        {
            AppDBContext.OnLogEntry -= AddLogEntry;
        }
    }
}
