using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.ViewModel
{
    public class CRUD_VM:ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        private readonly IDataService _service;
        [Reactive] public ObservableCollection<Sector> Sectors { get; set; } = new();
        [Reactive] public ObservableCollection<Sensor> Sensors { get; set; } = new();
        [Reactive] public ObservableCollection<Mechanism> Mechanisms { get; set; } = new();
        public CRUD_VM(IDataBaseProvider provider,IDataService service) 
        {
            _provider = provider;
            _service = service;
            Sectors = _service.Sectors;
            Sensors = _service.Sensors;
            Mechanisms = _service.Mechanisms;
        }
    }
}
