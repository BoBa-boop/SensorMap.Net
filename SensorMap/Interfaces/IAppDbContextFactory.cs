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
        AppDBContext CreateDbContext();
        void UpdateConnectionString(string path);
    }
}
