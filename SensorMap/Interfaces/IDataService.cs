using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Interfaces
{
    public interface IDataService
    {
        bool IsEditMode { get; set; }
        ObservableCollection<PLCManufacturer> Manufacturers { get; }
        ObservableCollection<SensorType> SensorTypes {  get; }
        ObservableCollection<Sensor> Sensors { get; }
        ObservableCollection<Sector> Sectors { get; }
        ObservableCollection<Mechanism> Mechanisms { get; }
        ObservableCollection<PLC> PLCs { get; }
        void UpdateCollection<T>(T entity) where T:class;
    }
}
