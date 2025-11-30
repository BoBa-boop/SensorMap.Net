using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SensorMap.Services
{
    public class DataBaseProvider : IDataBaseProvider
    {
        private readonly IAppDbContextFactory _dbContextFactory;
        public DataBaseProvider(IAppDbContextFactory dBContextFactory) 
        {
            _dbContextFactory = dBContextFactory;
        }

        public async Task Create<T>(T entity) where T : class
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                try
                {
                    dBContext.Entry<T>(entity).State = EntityState.Added;
                    await dBContext.SaveChangesAsync();
                    MessageBox.Show("Данные добавлены в Базу Данных!","Результат операции",MessageBoxButton.OK);
                }
                catch(DbUpdateException ex)
                {
                    var iner = ex.InnerException;
                    MessageBox.Show(iner.Message);
                }
            }
        }
        public async Task Delete<T>(T entity) where T : class
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                dBContext.Entry<T>(entity).State = EntityState.Deleted;
                await dBContext.SaveChangesAsync();
                MessageBox.Show("Данные удалены из Базы Данных!", "Результат операции", MessageBoxButton.OK);
            }
        }

        public async Task Update<T>(T entity) where T : class
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                dBContext.Entry<T>(entity).State = EntityState.Modified;
                await dBContext.SaveChangesAsync();
                MessageBox.Show("Данные обновлены в Базе Данных!", "Результат операции", MessageBoxButton.OK);
            }
        }
        public async Task<IEnumerable<Mechanism>> GetAllMechanisms()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.Mechanisms.Include(x=>x.Sector).ToListAsync();
            }
        }

        public async Task<IEnumerable<Sector>> GetAllSectorsAsync()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.Sectors.Include(x=>x.Mechanisms).ToListAsync();
            }
        }

        public async Task<IEnumerable<Sensor>> GetAllSensors()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.Sensors.Include(x=>x.SensorType).ToListAsync();
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

        public async Task<IEnumerable<SensorType>> GetSensortypeAsync()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.SensorTypes.ToListAsync();
            }
        }

        public async Task<IEnumerable<PLC>> GetAllPLCsAsync()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.PLCs.Include(x => x.Mechanisms).ToListAsync();
            }
        }
    }
}
