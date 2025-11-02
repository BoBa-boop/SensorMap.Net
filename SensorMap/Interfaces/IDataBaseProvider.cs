using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Interfaces
{
    public interface IDataBaseProvider
    {
        Task<IEnumerable<Sector>> GetAllSectorsAsync();
        Task<IEnumerable<Sensor>> GetAllSensors();
        Task<IEnumerable<SensorType>> GetSensortypeAsync();
        Task<T> GetElementByID<T>(int id) where T:class;
        Task<IEnumerable<Mechanism>> GetAllMechanisms();
        Task Create<T>(T entity) where T : class;
        Task Delete<T>(T entity) where T : class;
        Task Update<T>(T entity) where T : class;
    }
}
