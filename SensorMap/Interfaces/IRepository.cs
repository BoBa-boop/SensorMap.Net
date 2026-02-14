using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        event ChangedHandler ChangedEvent;
        Task<IEnumerable<TEntity>> GetAllData();
        Task<TEntity> GetById(int id);
        Task<IEnumerable<TEntity>> GetAllDataInclude(TEntity[] param);
        Task Create(TEntity entity);
        Task Update(TEntity entity);
        Task Delete(TEntity entity);
        Task UpdateRange(IEnumerable<TEntity> entityRange);
        
    }
    public delegate void ChangedHandler(object sender, ActionChanged action);
}
