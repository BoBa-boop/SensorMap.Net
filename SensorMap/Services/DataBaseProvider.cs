using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SensorMap.Interfaces;
using SensorMap.Logging;
using System.IO;
using System.Windows.Shapes;
using Path = System.IO.Path;

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

        public async Task CreateBackupDB(string backupDirectory)
        {
            string connection_string = Properties.Settings.Default.ConnectionString;
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(backupDirectory,$"DataBase_backup_{timestamp}.db");

            try
            {
                await Task.Run(() =>
                {
                    // Создаем соединение с исходной базой
                    using (SqliteConnection sourceConnection = new SqliteConnection(connection_string))
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
                });
                // Проверка, что файл реально создан и не пуст
                var info = new FileInfo(backupPath);
                    if (!info.Exists || info.Length == 0)
                        throw new InvalidOperationException("Файл бэкапа не создан или пуст.");

                    // UI-уведомление — явно в UI-потоке
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                        Growl.Success(new GrowlInfo
                        {
                            Message = "Резервная копия БД создана.",
                            CancelStr = "Ignore",
                            ShowDateTime = false,
                            WaitTime = 2
                        }));
                
            }
            catch (Exception ex)
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                Growl.Error(new GrowlInfo
                {
                    Message = $"Ошибка бэкапа: {ex.Message}",
                    ShowDateTime = false,
                    WaitTime = 5
                }));
                Logger.Error(ex);
            }
        }

        public IReadOnlyList<IDbCheck> CreateChecks()
        {
            throw new NotImplementedException();
        }
        public bool ApplyDataBase(string path)
        {
            throw new NotImplementedException();
        }
    }
}
