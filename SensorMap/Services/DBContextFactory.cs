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
        private string _connectionString;
        public DBContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public AppDBContext CreateDbContext()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                _connectionString = Path.Combine("DataSource=" + AppPaths.DataDir, "AppDataBase.db");
                Properties.Settings.Default.ConnectionString = _connectionString;
                Properties.Settings.Default.Save();
            }
            DbContextOptions options = new DbContextOptionsBuilder()
                .UseSqlite(_connectionString)
                .AddInterceptors(new AuditInterceptor())
                .Options;
            return new AppDBContext(options);
        }
        public AppDBContext CreateDbContext(string path)
        {
            var cs = new SqliteConnectionStringBuilder
            {
                DataSource = path,
                Mode = SqliteOpenMode.ReadOnly,
                Pooling = false
            }.ToString();
            
            DbContextOptions options = new DbContextOptionsBuilder()
                .UseSqlite(cs)
                .AddInterceptors(new AuditInterceptor())
                .Options;
            return new AppDBContext(options);
        }

        public void UpdateConnectionString(string path)
        {
            _connectionString = path;
        }
    }
}
