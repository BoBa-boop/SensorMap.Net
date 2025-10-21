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

            // получаем конфигурацию из файла appsettings.json
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");
            IConfigurationRoot config = builder.Build();

            // получаем строку подключения из файла appsettings.json
            string connectionString = config.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlite(connectionString);
            return new AppDBContext(optionsBuilder.Options);
        }
    }
}
