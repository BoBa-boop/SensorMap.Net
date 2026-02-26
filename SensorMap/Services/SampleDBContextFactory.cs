using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SensorMap.EF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Services
{
    public class SampleDBContextFactory : IDesignTimeDbContextFactory<AppDBContext>
    {
        public AppDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDBContext>();
            var connectionString = "Data Source=" + Path.Combine(AppContext.BaseDirectory, "AppDataBase.db");
            optionsBuilder.UseSqlite(connectionString);
            return new AppDBContext(optionsBuilder.Options);
        }
    }
}
