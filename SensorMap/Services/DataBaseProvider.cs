using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
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
                    Growl.Success(new GrowlInfo
                    {
                        Message = "Добавлено в Базу Данных!",
                        CancelStr = "Ignore",
                        ShowDateTime = false,
                        WaitTime = 2
                    });
                }
                catch(DbUpdateException ex)
                {
                    var iner = ex.InnerException;
                    System.Windows.MessageBox.Show(iner.Message);
                }
            }
        }
        public async Task AddSensorAssignmentAsync(SensorAssignments assignment)
        {
            using (AppDBContext _context = _dbContextFactory.CreateDbContext())
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // ПРОВЕРЬТЕ, что связанные объекты уже сохранены в БД
                    var sensorExists = await _context.Sensors.AnyAsync(s => s.Id == assignment.SensorId);
                    //var plcExists = await _context.PLCs.AnyAsync(p => p.Id == assignment.PLCId);
                    var mechanismExists = await _context.Mechanisms.AnyAsync(m => m.Id == assignment.MechanismId);

                    if (!sensorExists)
                    {
                        Growl.Error(new GrowlInfo
                        {
                            Message = $"[SYSTEM] Не найден датчик id:{assignment.SensorId}",
                            CancelStr = "Ignore",
                            ShowDateTime = false,
                            Type = InfoType.Error,
                            WaitTime = 2
                        });
                    }



                    // Убедитесь, что объекты отслеживаются
                    if (assignment.Sensor != null && _context.Entry(assignment.Sensor).State == EntityState.Detached)
                    {
                        _context.Attach(assignment.Sensor);
                    }

                    if (assignment.Mechanism != null && _context.Entry(assignment.Mechanism).State == EntityState.Detached)
                    {
                        _context.Attach(assignment.Mechanism);
                    }

                    _context.SensorAssignments.Add(assignment);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Debug.WriteLine($"Ошибка при добавлении SensorAssignment: {ex.Message}");
                    throw;
                }
            }
        }
        public async Task Delete<T>(T entity) where T : class
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                dBContext.Entry<T>(entity).State = EntityState.Deleted;
                await dBContext.SaveChangesAsync();
                Growl.Success(new GrowlInfo
                {
                    Message = "Удалено из Базы Данных!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
        }
        public async Task Update<T>(T entity) where T : class
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                dBContext.Entry<T>(entity).State = EntityState.Modified;
                await dBContext.SaveChangesAsync();
                Growl.Success(new GrowlInfo
                {
                    Message = "Обновление в Базе Данных!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
        }
        public async Task<IEnumerable<Mechanism>> GetAllMechanisms()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.Mechanisms.Include(x=>x.Sector)
                    .Include(x=>x.SensorsAssig)
                    .ThenInclude(x=>x.Sensor)
                    .ThenInclude(sen=>sen.SensorType)
                    .ToListAsync();
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

        public async Task<IEnumerable<PLCManufacturer>> GetManufacturersAsync()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.PLC_Manufacturers.ToListAsync();
            }
        }
    }
}
