using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                Growl.Warning("Просмотр данных недоступен. Загрузите БД в приложение!");
                throw new Exception("Строка подключения не может быть пустой");
            }
            DbContextOptions options = new DbContextOptionsBuilder()
                .UseSqlite(_connectionString)
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
