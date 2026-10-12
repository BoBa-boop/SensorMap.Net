using Microsoft.Extensions.Configuration;
using SensorMap.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            IConfigurationRoot config = AppSettingBuilder();
            if (string.IsNullOrEmpty(connection_string)) connection_string = Path.Combine(DataDir,config.GetConnectionString("DefaultConnection")??string.Empty);
            
            return connection_string;
        }
        private static IConfigurationRoot AppSettingBuilder()
        {
            var builder = new ConfigurationBuilder();
            // установка пути к текущему каталогу
            builder.SetBasePath(Directory.GetCurrentDirectory());
            // получаем конфигурацию из файла appsettings.json
            builder.AddJsonFile("appsettings.json");
            // создаем конфигурацию
            var config = builder.Build();
            return config;
        }

    }
}
