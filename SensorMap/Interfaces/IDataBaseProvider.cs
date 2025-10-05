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
        Task<ObservableCollection<Sector>> GetAllSectorsAsync();
        IEnumerable<Sensor> GetAllSensors();
        Task CreateSector(Sector sector);
    }
}
