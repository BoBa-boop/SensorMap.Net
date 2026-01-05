using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.EntityFrameworkCore;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using System.Diagnostics;

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

        public async Task AddSensorsAssignmentAsync(IEnumerable<SensorAssignments> sensors)
        {
            using (AppDBContext dBContext = _dbContextFactory.CreateDbContext())
            {
                using var transaction = await dBContext.Database.BeginTransactionAsync();

                try
                {
                    foreach (SensorAssignments sensor in sensors)
                    {
                        if (sensor.Id == 0)
                        {
                            // ПРОВЕРЬТЕ, что связанные объекты уже сохранены в БД
                            if (!await dBContext.Sensors.AnyAsync(s => s.Id == sensor.SensorId))
                            {
                                Growl.Error(new GrowlInfo
                                {
                                    Message = $"[SYSTEM] Не найден датчик id:{sensor.SensorId}",
                                    CancelStr = "Ignore",
                                    ShowDateTime = false,
                                    Type = InfoType.Error,
                                    WaitTime = 2
                                });
                                throw new Exception($"Не найден датчик id:{sensor.SensorId} в базе данных");
                            }
                            if (!await dBContext.Mechanisms.AnyAsync(m => m.Id == sensor.MechanismId))
                            {
                                Growl.Error(new GrowlInfo
                                {
                                    Message = $"[SYSTEM] Не найден механизм id:{sensor.MechanismId}",
                                    CancelStr = "Ignore",
                                    ShowDateTime = false,
                                    Type = InfoType.Error,
                                    WaitTime = 2
                                });
                                throw new Exception($"Не найден механизм id:{sensor.MechanismId} в базе данных");
                            }
                            if (!await dBContext.PLCs.AnyAsync(p => p.Id == sensor.PLCId))
                            {
                                Growl.Error(new GrowlInfo
                                {
                                    Message = $"[SYSTEM] Не найден PLC id:{sensor.PLCId}",
                                    CancelStr = "Ignore",
                                    ShowDateTime = false,
                                    Type = InfoType.Error,
                                    WaitTime = 2
                                });
                                throw new Exception($"Не найден PLC id:{sensor.PLCId} в базе данных");
                            }

                            // Важно: сначала получить сущности из контекста, а не прикреплять внешние
                            var existingMechanism = await dBContext.Mechanisms
                                .FirstOrDefaultAsync(m => m.Id == sensor.MechanismId);

                            var existingSensor = await dBContext.Sensors
                                .FirstOrDefaultAsync(s => s.Id == sensor.SensorId);

                            if (existingMechanism == null || existingSensor == null)
                            {
                                throw new Exception("Не найдены связанные сущности в базе данных");
                            }

                            // Присваиваем отслеживаемые сущности
                            sensor.Mechanism = existingMechanism;
                            sensor.Sensor = existingSensor;

                            // Добавляем assignment (связанные сущности уже отслеживаются)
                            dBContext.SensorAssignments.Add(sensor);
                        }
                        else
                        {
                            // Обновление существующей записи
                            var currentSensor = await dBContext.SensorAssignments
                                .FirstOrDefaultAsync(a => a.Id == sensor.Id);

                            if (currentSensor == null)
                            {
                                Growl.Error(new GrowlInfo
                                {
                                    Message = $"[SYSTEM] Не найден датчик id:{sensor.Id}",
                                    CancelStr = "Ignore",
                                    ShowDateTime = false,
                                    Type = InfoType.Error,
                                    WaitTime = 2
                                });
                                continue;
                            }
                            dBContext.Entry<SensorAssignments>(sensor).State = EntityState.Modified;
                        }
                        await dBContext.SaveChangesAsync();                        
                        await transaction.CommitAsync();
                        Growl.Success(new GrowlInfo
                        {
                            Message = "Сохранено в Базу Данных!",
                            CancelStr = "Ignore",
                            ShowDateTime = false,
                            WaitTime = 2
                        });
                    }
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
                dBContext.Mechanisms.Local.ToObservableCollection();
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
                    .Include(m=>m.PLC)
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
