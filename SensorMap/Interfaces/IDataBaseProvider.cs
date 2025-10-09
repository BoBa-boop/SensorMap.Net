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
        Task<T> GetElementByID<T>(int id) where T:class;
        Task<IEnumerable<Mechanism>> GetAllMechanisms();
        Task CreateSector(Sector sector);
        Task CreateSensor(Sensor sensor);
        Task CreateMechainsm(Mechanism mechanism);
    }
}
