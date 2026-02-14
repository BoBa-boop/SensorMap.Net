using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.EntityFrameworkCore;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Repositories
{
    public class SensorTypeRepository : IRepository<SensorType>
    {
        public event ChangedHandler ChangedEvent;
        private AppDBContext _dbContext;

        public SensorTypeRepository(AppDBContext dBContext)
        {
            _dbContext = dBContext;
        }
        public async Task Create(SensorType entity)
        {
            try
            {
                _dbContext.Entry(entity).State = EntityState.Added;
                await _dbContext.SaveChangesAsync();
                Growl.Success(new GrowlInfo
                {
                    Message = "Добавлено в Базу Данных!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
            catch (Exception ex)
            {
                Growl.Fatal(new GrowlInfo
                {
                    Message = "Ошибка при добавление!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
        }

        public async Task Delete(SensorType entity)
        {
            try
            {
                _dbContext.Entry(entity).State = EntityState.Deleted;
                await _dbContext.SaveChangesAsync();
                Growl.Success(new GrowlInfo
                {
                    Message = "Удалено из Базы Данных!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
            catch (Exception ex)
            {
                Growl.Fatal(new GrowlInfo
                {
                    Message = "Ошибка при удаление!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
        }

        public async Task<IEnumerable<SensorType>> GetAllData()
        {
            return await _dbContext.Set<SensorType>().ToListAsync();
        
        }

        public async Task<IEnumerable<SensorType>> GetAllDataInclude(SensorType[] param)
        {
            IQueryable<SensorType> queryable = _dbContext.Set<SensorType>();

            foreach (var property in param)
                queryable = queryable.Include(x => x == property);

            return await queryable.ToListAsync();
        }

        public async Task<SensorType> GetById(int id)
        {
            var res = await _dbContext.Set<SensorType>().FindAsync(id);
            if (res == null)
                throw new NullReferenceException();
            return res;
        }

        public async Task Update(SensorType entity)
        {
            try
            {
                _dbContext.Update(entity);
                await _dbContext.SaveChangesAsync();
                Growl.Success(new GrowlInfo
                {
                    Message = "Обновлено в Базе Данных!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
            catch (Exception ex)
            {
                Growl.Fatal(new GrowlInfo
                {
                    Message = "Ошибка при удаление!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
        }

        public async Task UpdateRange(IEnumerable<SensorType> entityRange)
        {
            try
            {
                _dbContext.UpdateRange(entityRange);
                await _dbContext.SaveChangesAsync();
                Growl.Success(new GrowlInfo
                {
                    Message = "Обновлено в Базе Данных!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
            catch (Exception ex)
            {
                Growl.Fatal(new GrowlInfo
                {
                    Message = "Ошибка при обновление!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
        }
    }
}
