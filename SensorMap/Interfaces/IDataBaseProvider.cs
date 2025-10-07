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
        Task<IEnumerable<Mechanism>> GetAllMechanisms();
        Task CreateSector(Sector sector);
    }
}
