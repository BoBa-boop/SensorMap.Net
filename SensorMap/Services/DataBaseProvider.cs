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

        public async Task<IEnumerable<Mechanism>> GetAllMechanisms()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.Mechanisms.ToListAsync();
            }
        }

        public async Task<IEnumerable<Sector>> GetAllSectorsAsync()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.Sectors.ToListAsync();
            }
        }

        public async Task<IEnumerable<Sensor>> GetAllSensors()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.Sensors.ToListAsync();
            }
        }
    }
}
