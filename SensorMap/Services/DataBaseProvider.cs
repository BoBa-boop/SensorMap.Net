using Microsoft.EntityFrameworkCore;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Services
{
    public class DataBaseProvider : IDataBaseProvider
    {
        private readonly IAppDbContextFactory _dbContextFactory;
        public DataBaseProvider(IAppDbContextFactory dBContextFactory) 
        {
            _dbContextFactory = dBContextFactory;
        }

        public async Task CreateSector(Sector sector)
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                dBContext.Sectors.Add(sector);
                await dBContext.SaveChangesAsync();
            }
        }

        public async Task<ObservableCollection<Sector>> GetAllSectorsAsync()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return new ObservableCollection<Sector>(await dBContext.Sectors.ToListAsync());
            }
        }

        public IEnumerable<Sensor> GetAllSensors()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return dBContext.Sensors.ToList();
            }
        }
    }
}
