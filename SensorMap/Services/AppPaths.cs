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
            
        public static string GetDbPath()
        {
            string? connection_string = Settings.Default.ConnectionString;
            if (string.IsNullOrEmpty(Settings.Default.ConnectionString))
                return string.Empty;
            return Path.Combine(DataDir, GetDbName());
        }
        public static string GetDbName()
        {
            if(string.IsNullOrEmpty(Settings.Default.ConnectionString))
                return string.Empty;
            var csb = new SqliteConnectionStringBuilder(Settings.Default.ConnectionString);
            return csb.DataSource;
        }
        

    }
}
