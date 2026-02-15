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
        Mechanism CurrentMechanism_Global { get; set; }
        Sector CurrentSector_Global { get; set; }
        bool IsEditMode { get; set; }
    }
}
