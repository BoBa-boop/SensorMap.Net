using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using NLog.Config;
using SensorMap.Interfaces;
using SensorMap.Logging;
using System.IO;
using System.Text;
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
            CreateChecks();
            _dbContextFactory.UpdateConnectionString(path);
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

        public IReadOnlyList<IDbCheck> CreateChecks()=>new List<IDbCheck>
        {
            new FileAccessCheck(),                          //файл доступен
            new SqliteHeaderCheck(),                        //формат SQLite
            new IntegrityCheck(),                           //целостность
            new ForeignKeyCheck(),                          //внешние ключи
            new EfConnectionCheck(_dbContextFactory),       //подключение EF
            new MigrationsTableCheck(),                     //таблица миграций
            new MigrationVersionCheck(_dbContextFactory),   //версия схемы
            new ModelTablesCheck(_dbContextFactory),        //таблица модели
            new WriteAccessCheck()                          //права на запись
        };
    }

    internal class WriteAccessCheck : IDbCheck
    {
        public string Title => "Права на запись";

        public Task<CheckResult> RunAsync(string path) => Task.Run(() =>
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
                return new CheckResult(true);
            }
            catch (UnauthorizedAccessException) { return new CheckResult(false, "Нет прав на запись файла"); }
        });
    }

    internal class ModelTablesCheck : IDbCheck
    {
        private readonly IAppDbContextFactory dbContextFactory;
        public ModelTablesCheck(IAppDbContextFactory appDbContextFactory)
        {
            dbContextFactory = appDbContextFactory;
        }

        public string Title => "Соответствие модели таблиц";

        public Task<CheckResult> RunAsync(string path) => Task.Run(async() =>
        {
            try
            {
                using var db = dbContextFactory.CreateDbContext(DbCheckHelper.ReadOnlyConnectionString(path));

                // 1. Что ожидает модель: таблица -> набор столбцов
                var expected = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var entity in db.Model.GetEntityTypes())
                {
                    var table = entity.GetTableName();
                    if (table == null) continue; // сущность без таблицы (например, view)

                    var storeId = StoreObjectIdentifier.Table(table, entity.GetSchema());
                    if (!expected.TryGetValue(table, out var cols))
                        expected[table] = cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var prop in entity.GetProperties())
                    {
                        var col = prop.GetColumnName(storeId);
                        if (col != null) cols.Add(col);
                    }
                }

                // 2. Что есть в БД
                var conn = new SqliteConnection(DbCheckHelper.ReadOnlyConnectionString(path));
                await conn.OpenAsync();

                var actualTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                await using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%';";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync()) actualTables.Add(reader.GetString(0));
                }

                var problems = new List<string>();

                foreach (var (table, columns) in expected)
                {
                    if (!actualTables.Contains(table))
                    {
                        problems.Add($"нет таблицы {table}");
                        continue;
                    }

                    var actualCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    await using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT name FROM pragma_table_info($t);";
                        var p = cmd.CreateParameter();
                        p.ParameterName = "$t";
                        p.Value = table;
                        cmd.Parameters.Add(p);

                        await using var reader = await cmd.ExecuteReaderAsync();
                        while (await reader.ReadAsync()) actualCols.Add(reader.GetString(0));
                    }

                    var missing = columns.Where(c => !actualCols.Contains(c)).ToList();
                    if (missing.Count > 0)
                        problems.Add($"{table}: нет столбцов {string.Join(", ", missing)}");
                }

                return problems.Count == 0
                ? new CheckResult(true)
                : new CheckResult(false, "Структура БД не соответствует модели: " + string.Join("; ", problems));
            }
            catch (Exception ex)
            {
                return new CheckResult(false, $"Ошибка проверки структуры: {ex.Message}");
            }
        });
    }

    internal class MigrationVersionCheck : IDbCheck
    {
        public string Title => "Версия миграций БД";
        private readonly IAppDbContextFactory dbContextFactory;
        public MigrationVersionCheck(IAppDbContextFactory appDbContextFactory)
        {
            dbContextFactory = appDbContextFactory;
        }
        public Task<CheckResult> RunAsync(string path) => Task.Run(() =>
        {
            using (var dbContext = dbContextFactory.CreateDbContext(path))
            {
                var applied = (dbContext.Database.GetAppliedMigrations()).ToList();
                var known = dbContext.Database.GetMigrations().ToList();

                if (applied.Except(known).Any())
                    return new CheckResult(false, "БД содержит новую миграцию, которой нет в текущей. Обновите приложение.");
                if (known.Except(applied).Any())
                    return new CheckResult(false, "БД необходимо обновить до текущей версии приложения.");
                return new CheckResult(true);
            }
        });
    }

    internal class MigrationsTableCheck : IDbCheck
    {
        public string Title => "Таблица миграция";

        public Task<CheckResult> RunAsync(string path)=> Task.Run(() =>
        {
            return new CheckResult(true);
        });
    }

    internal class EfConnectionCheck : IDbCheck
    {
        public string Title => "Подключение к БД";
        private readonly IAppDbContextFactory dbContextFactory;
        public EfConnectionCheck(IAppDbContextFactory appDbContextFactory)
        {
            dbContextFactory = appDbContextFactory;
        }
        public Task<CheckResult> RunAsync(string path)=> Task.Run(async () =>
        {
            try
            {
                using (var dbContext = dbContextFactory.CreateDbContext(path))
                {
                    if (!await dbContext.Database.CanConnectAsync())
                        return new CheckResult(false, "Не удалось подключиться к БД");
                    
                    using var connecton = new SqliteConnection(DbCheckHelper.ReadOnlyConnectionString(path));
                    await connecton.OpenAsync();
                    await using var cmd = connecton.CreateCommand();
                    cmd.CommandText = "SELECT count(*) FROM sqlite_master;";
                    await cmd.ExecuteScalarAsync();
                    await connecton.CloseAsync();
                    return new CheckResult(true);
                }
            }
            catch (Exception ex)
            {
                return new CheckResult(false,$"Ошибка подключения {ex.Message}");
            }
        });
    }

    internal class ForeignKeyCheck : IDbCheck
    {
        public string Title => "Внешние ключи";

        public Task<CheckResult> RunAsync(string path) => Task.Run(() =>
        {
            
            using var connecton = new SqliteConnection(DbCheckHelper.ReadOnlyConnectionString(path));
            connecton.Open();
            using var cmd = connecton.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_key_check;";
            using var reader = cmd.ExecuteReader();
            bool hasViolations = reader.Read();
            connecton.Close();
            return hasViolations ? new CheckResult(false, "Найдены нарушения внешних ключей") : new CheckResult(true);
        });
    }

    internal class IntegrityCheck : IDbCheck
    {
        public string Title => "Целостность БД";

        public Task<CheckResult> RunAsync(string path) => Task.Run(() =>
        {
            
            using var connecton = new SqliteConnection(DbCheckHelper.ReadOnlyConnectionString(path));
            connecton.Open();
            using var cmd = connecton.CreateCommand();
            cmd.CommandText = "PRAGMA quick_check;";
            var result = cmd.ExecuteScalar() as string;
            connecton.Close();
            return result=="ok" ? new CheckResult(true):new CheckResult(false,$"БД повреждена {result}");
        });
    }

    internal class SqliteHeaderCheck : IDbCheck
    {
        public string Title => "Формат БД";

        public Task<CheckResult> RunAsync(string path) => Task.Run(() =>
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read,FileShare.ReadWrite);
            var buffer = new byte[16];
            int read = fs.Read(buffer, 0, 16);
            bool ok = read == 16 && Encoding.ASCII.GetString(buffer, 0, 15) == "SQLite format 3";

            return new CheckResult(ok,ok?null:"Файл не является БД SQLite");
        });
    }

    internal class FileAccessCheck : IDbCheck
    {
        public string Title => "Файл существует и доступен";

        public Task<CheckResult> RunAsync(string path) => Task.Run(() =>
        {
            if (!File.Exists(path))
                return new CheckResult(false,"Файл не найден");
            if (new FileInfo(path).Length==0)
                return new CheckResult(false, "Файл пуст");

            try
            {
                using var fs = new FileStream(path,FileMode.Open,FileAccess.Read);
                return new CheckResult(true);
            }
            catch (IOException) { return new CheckResult(false,"Файл занят другим процессом"); }
            catch (UnauthorizedAccessException) { return new CheckResult(false,"Нет прав на чтение файла"); }
        });
    }

    internal static class DbCheckHelper
    {
        public static string ReadOnlyConnectionString(string path) =>
            new SqliteConnectionStringBuilder
            {
                DataSource = path,
                Mode = SqliteOpenMode.ReadOnly,
                Pooling = false
            }.ToString();
    }
}
