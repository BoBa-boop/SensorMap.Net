using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Logging;
using SensorMap.Model;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SensorMap.Services
{
    public class DataBaseProvider : IDataBaseProvider
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IAppDbContextFactory _dbContextFactory;

        public DataBaseProvider(IAppDbContextFactory dBContextFactory) 
        {
            _dbContextFactory = dBContextFactory;
            
        }
        public bool ChangeDataBase(string path)
        {
            string newDB_connectionString = "DataSource=" + path;

            _dbContextFactory.UpdateConnectionString(newDB_connectionString);
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                if (!dbContext.Database.CanConnect())
                    throw new Exception("База Данных не имеет таблиц для сохранения данных");
                var applied = (dbContext.Database.GetAppliedMigrations()).ToList();
                var known = dbContext.Database.GetMigrations().ToList();
                var unknow = applied.Except(known).ToList();
                if (unknow.Count > 0)
                {
                    var reason = SaveErrorExplainer.Explain(new Exception("Обновите приложение для взаимодействия с выбранной БД"));
                    string message = "БД содержит миграции(данные) не записанные в текущей версии приложения";
                    Growl.Error(message);
                    Logger.Error(message,reason);
                    return false;
                }
                if (known.Except(applied).Count() > 0)
                {
                    Growl.Error("БД необходимо обновить до текущей версии приложения.");
                    return false;
                }
            }
            Properties.Settings.Default.ConnectionString = newDB_connectionString;
            Properties.Settings.Default.Save();
            return true;
        }

        public void CreateBackupDB(string backupDirectory)
        {
            string connection_string = Properties.Settings.Default.ConnectionString;

            // Генерируем имя файла для бэкапа
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(backupDirectory,$"DataBase_backup_{timestamp}.db");

            // Создаем соединение с исходной базой
            using(SqliteConnection sourceConnection = new SqliteConnection(connection_string))
            {
                sourceConnection.Open();

                // Создаем соединение для бэкапа
                using (var backupConnection = new SqliteConnection($"Data Source={backupPath}"))
                {
                    backupConnection.Open();

                    // Выполняем бэкап
                    sourceConnection.BackupDatabase(backupConnection);
                    backupConnection.Close();
                }
                sourceConnection.Close();
            }
            if (File.Exists(backupPath))
            {
                Growl.Success(new GrowlInfo
                {
                    Message = "Резервная копия БД создана.",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
        }
        
    }
}
