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
        ObservableCollection<SensorType> SensorTypes {  get; }
        ObservableCollection<Sensor> Sensors { get; }
        ObservableCollection<Sector> Sectors { get; }
        ObservableCollection<Mechanism> Mechanisms { get; }
    }
}
