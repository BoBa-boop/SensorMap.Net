using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using SensorMap.Properties;
using System.IO;

namespace SensorMap.Services
{
    public static class AppPaths
    {
        //%APPDATA%/Roaming
        public static readonly string DataDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SensorMap");
        
        public static readonly string SettingsFile = Path.Combine(DataDir, "SettingsApp.json");

        public static readonly string DefaultDbPath = Path.Combine(DataDir, "AppDataBase.db");

        /// <summary>Путь к БД из настроек. Пустая строка, если пользователь ещё ничего не выбирал.</summary>
        public static string GetDbPath() => Settings.Default.CurrentDbPath ?? string.Empty;

        public static string GetDbName()
        {
            var path = GetDbPath();
            return string.IsNullOrEmpty(path) ? string.Empty : Path.GetFileName(path);
        }
        public static void SaveDbPath(string path)
        {
            Settings.Default.CurrentDbPath = path;
            Settings.Default.Save();
        }

    }
}
