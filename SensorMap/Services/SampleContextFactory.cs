using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SensorMap.EF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Services
{///создает БД в проекте, не в папке. И данные не сохраняются 
    public class SampleContextFactory : IDesignTimeDbContextFactory<AppDBContext>
    {
        public AppDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDBContext>();

            var config = new ConfigurationBuilder()
                                   .AddJsonFile("appsettings.json")
                                   .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                                   .Build();
            string? connection_string = config.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlite(connection_string);
            return new AppDBContext(optionsBuilder.Options);
        }
    }
}
