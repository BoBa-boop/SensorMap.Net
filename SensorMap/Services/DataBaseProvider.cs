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

        public async Task CreateMechainsm(Mechanism mechanism)
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                dBContext.Mechanisms.Add(mechanism);
                await dBContext.SaveChangesAsync();
            }
        }

        public async Task CreateSector(Sector sector)
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                dBContext.Sectors.Add(sector);
                await dBContext.SaveChangesAsync();
            }
        }

        public async Task CreateSensor(Sensor sensor)
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                dBContext.Sensors.Add(sensor);
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

        public async Task<T> GetElementByID<T>(int id) where T : class 
        {
            try
            {
                using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
                {
                    var res = await dBContext.Set<T>().FindAsync(id);
                    if (res == null)
                        throw new NullReferenceException();
                    return res;
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                HandyControl.Controls.MessageBox.Show($"Ошибка при поиске элемента {typeof(T).Name} с ID {id}: {ex.Message}");
                return null;
            }
        }
    }
}
