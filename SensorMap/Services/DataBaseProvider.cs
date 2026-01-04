using DynamicData;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
using INavigation = Microsoft.EntityFrameworkCore.Metadata.INavigation;

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
                    DataBaseEvents.RaiseEntityCreated(entity);
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
                        return;
                    }

                    if (!mechanismExists)
                    {
                        Growl.Error(new GrowlInfo
                        {
                            Message = $"[SYSTEM] Не найден механизм id:{assignment.MechanismId}",
                            CancelStr = "Ignore",
                            ShowDateTime = false,
                            Type = InfoType.Error,
                            WaitTime = 2
                        });
                        return;
                    }

                    // Важно: сначала получить сущности из контекста, а не прикреплять внешние
                    var existingMechanism = await _context.Mechanisms
                        .FirstOrDefaultAsync(m => m.Id == assignment.MechanismId);

                    var existingSensor = await _context.Sensors
                        .FirstOrDefaultAsync(s => s.Id == assignment.SensorId);

                    if (existingMechanism == null || existingSensor == null)
                    {
                        throw new Exception("Не найдены связанные сущности в базе данных");
                    }

                    // Присваиваем отслеживаемые сущности
                    assignment.Mechanism = existingMechanism;
                    assignment.Sensor = existingSensor;

                    // Добавляем assignment (связанные сущности уже отслеживаются)
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
                DataBaseEvents.RaiseEntityDeleted(entity);
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
                DataBaseEvents.RaiseEntityUpdated(entity);
            }
        }
        public async Task<IEnumerable<Mechanism>> GetAllMechanisms()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.Mechanisms.Include(x=>x.Sector)
                    .AsNoTracking()
                    .Include(x=>x.SensorsAssig)
                    .ThenInclude(x=>x.Sensor)
                    .ThenInclude(sen=>sen.SensorType)
                    .ToListAsync();
            }
        }
        public async Task<IEnumerable<Mechanism>> GetAllMechanismsWithSector()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.Mechanisms.Include(x => x.Sector).ToListAsync();
            }
        }

        public async Task<IEnumerable<Sector>> GetAllSectorsAsync()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.Sectors.Include(x=>x.Mechanisms).AsNoTracking().ToListAsync();
            }
        }

        public async Task<IEnumerable<Sensor>> GetAllSensors()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.Sensors.Include(x => x.SensorType).AsNoTracking().ToListAsync();
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
                return await dBContext.SensorTypes.AsNoTracking().ToListAsync();
            }
        }

        public async Task<IEnumerable<PLC>> GetAllPLCsAsync()
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                return await dBContext.PLCs.Include(x => x.Mechanisms).AsNoTracking().ToListAsync();
            }
        }

        
    }
}
