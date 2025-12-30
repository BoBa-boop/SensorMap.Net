using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Tools;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using SensorMap.View;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace SensorMap.ViewModel
{
    public class SensorVM:ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        private readonly IDataService _service;
        private Sensor _sensorsTreeNode;
        [Reactive]public Sensor SelectedNode
        {
            get => _sensorsTreeNode;
            set { this.RaiseAndSetIfChanged(ref _sensorsTreeNode, value); }
        }
        [Reactive]public ObservableCollection<Sensor> Sensors {  get; set; }
        [Reactive] public TreeViewCollection<SensorType, Sensor> SensorsTree { get; set; }
        private ObservableCollection<SensorType> sensorTypes {  get; set; }
        public SensorVM(IDataBaseProvider provider, IDataService service,Sensor sensor=null)
        {
            SelectedNode = sensor;
            _service = service;
            _provider = provider;
            sensorTypes = _service.SensorTypes;
            Sensors = _service.Sensors;
            Func<SensorType, Sensor,bool> filter = (type, sensor) => sensor.SensorTypeID == type.Id;
            SensorsTree = new TreeViewCollection<SensorType, Sensor>("Name",sensorTypes, Sensors, filter);
        }
        
    }
}
