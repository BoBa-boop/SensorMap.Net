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
    public class PLC_Repository : IRepository<PLC>
    {
        public event ChangedHandler ChangedEvent;

        private AppDBContext _dbContext;

        public PLC_Repository(AppDBContext dBContext)
        {
            _dbContext = dBContext;
        }
        public async Task Create(PLC entity)
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

        public async Task Delete(PLC entity)
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

        public virtual async Task<IEnumerable<PLC>> GetAllData()
        {
            return await _dbContext.Set<PLC>().ToListAsync();
        }

        public virtual async Task<IEnumerable<PLC>> GetAllDataInclude(PLC[] param)
        {
            IQueryable<PLC> queryable = _dbContext.Set<PLC>();

            foreach (var property in param)
                queryable = queryable.Include(x => x == property);

            return await queryable.ToListAsync();
        }

        public async virtual Task<PLC> GetById(int id)
        {
            var res = await _dbContext.Set<PLC>().FindAsync(id);
            if (res == null)
                throw new NullReferenceException();
            return res;
        }

        public async Task Update(PLC entity)
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
                    Message = "Ошибка при обновление!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
        }

        public async Task UpdateRange(IEnumerable<PLC> entityRange)
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
