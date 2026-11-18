using HandyControl.Controls;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Logging;
using System.IO;

namespace SensorMap.Services
{
    public class DBContextFactory : IAppDbContextFactory
    {
        private string _dbPath;

        public DBContextFactory(string dbPath) => _dbPath = dbPath;

        public string DbPath => _dbPath;

        public void SetDbPath(string path)
        {
            _dbPath = path;
            SqliteConnection.ClearAllPools(); // освободить файл старой БД
        }
        private static AppDBContext Create(string connectionString)
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlite(connectionString)
                .AddInterceptors(new AuditInterceptor())
                .Options;
            return new AppDBContext(options);
        }
        private static string BuildConnectionString(string path, SqliteOpenMode mode) =>
        new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = mode,
            Pooling = false
        }.ToString();
        public AppDBContext CreateDbContext() =>
        Create(BuildConnectionString(_dbPath, SqliteOpenMode.ReadWrite));

        public AppDBContext CreateReadableContext(string dbPath) =>
         Create(BuildConnectionString(dbPath, SqliteOpenMode.ReadOnly));
        public AppDBContext CreateWritableDbContext(string dbPath) =>
         Create(BuildConnectionString(dbPath, SqliteOpenMode.ReadWrite));
        public AppDBContext CreateCreatableDbContext() =>
        Create(BuildConnectionString(_dbPath, SqliteOpenMode.ReadWriteCreate));
    }
}
