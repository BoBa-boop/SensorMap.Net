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
    public class SensorVM:ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        [Reactive] public ObservableCollection<Sensor> Sensors 
        {
            get; 
            set; 
        }
        public SensorVM(IDataBaseProvider provider)
        {
            _provider = provider;
            Sensors = new ObservableCollection<Sensor>(_provider.GetAllSensors());
            
        }
    }
}
