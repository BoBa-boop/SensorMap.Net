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
    public class SectorRepository : IRepository<Sector>
    {
        private readonly AppDBContext _dbContext;

        public event ChangedHandler ChangedEvent;

        public SectorRepository(AppDBContext dBContext)
        {
            _dbContext = dBContext;

        }
        public async Task Create(Sector entity)
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
        public async Task Update(Sector entity)
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
        public async Task Delete(Sector entity)
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

        public virtual async Task<IEnumerable<Sector>> GetAllData()
        {
            return await _dbContext.Set<Sector>().ToListAsync();            
        }

        public virtual async Task<IEnumerable<Sector>> GetAllDataInclude(Sector[] param)
        {
            IQueryable<Sector> queryable = _dbContext.Set<Sector>();

            foreach (var property in param)
                queryable = queryable.Include(x => x == property);

            return await queryable.ToListAsync();
        }

        public virtual async Task<Sector> GetById(int id)
        {
            var res = await _dbContext.Set<Sector>().FindAsync(id);
            if (res == null)
                throw new NullReferenceException();
            return res;
        }

        public async Task UpdateRange(IEnumerable<Sector> entityRange)
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
