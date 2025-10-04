using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SensorMap.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Services
{
    public class DBContextFactory
    {
        private readonly string _connectionString;
        public DBContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public AppDBContext CreateDbContext()
        {
            DbContextOptions options = new DbContextOptionsBuilder().UseSqlite(_connectionString).Options;
            return new AppDBContext(options);
        }
    }
}
