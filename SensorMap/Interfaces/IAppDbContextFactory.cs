using SensorMap.EF;
using SensorMap.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Interfaces
{
    public interface IAppDbContextFactory
    {
        string DbPath { get; }
        AppDBContext CreateReadableContext(string path);
        AppDBContext CreateDbContext();
        AppDBContext CreateWritableDbContext(string path);
        AppDBContext CreateCreatableDbContext(); 
        void SetDbPath(string path);
    }
}
