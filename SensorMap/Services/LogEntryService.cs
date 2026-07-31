using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace SensorMap.Services
{
    public class LogEntryService : ILogEntryService, IDisposable
    {
        public ObservableCollection<DbActionLogs> Logs { get; } = new();

        public LogEntryService()
        {
            AppDBContext.OnLogEntry += AddLogEntry;
            LoadFromFile();
        }

        public void LoadFromFile(string path = "LogDb.txt")
        {
            if (!File.Exists(path)) return;
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var parts = line.Split('|', 4);
                if (parts.Length < 4) continue;
                if (!DateTime.TryParse(parts[0], out var ts)) continue;
                Logs.Add(new DbActionLogs { Timestamp = ts, Description = parts[3] });
            }
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
