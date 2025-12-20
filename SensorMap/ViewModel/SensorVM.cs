using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Tools;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Model.TreeNode;
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
        [Reactive]public ObservableCollection<SensorsTreeNode> Sensors {  get; set; }
        private ObservableCollection<SensorType> sensorTypes {  get; set; }
        public SensorVM(IDataBaseProvider provider, IDataService service,Sensor sensor=null)
        {
            SelectedNode = sensor;
            _service = service;
            _provider = provider;
            sensorTypes = _service.SensorTypes;
            Sensors = new ObservableCollection<SensorsTreeNode>(TreeNodeSensors());
        }

        private IEnumerable<SensorsTreeNode> TreeNodeSensors()
        {
            var mainNodes = new ObservableCollection<SensorsTreeNode>();
            var types = sensorTypes;
            foreach (var type in types)
            {
                mainNodes.Add(new SensorsTreeNode()
                {
                    Name = type.Name
                });
            }
            foreach (var sensor in _service.Sensors)
            {
                var node = mainNodes.FirstOrDefault(nodes => nodes.Name == sensor.SensorType.Name.ToString());
                node?.Children.Add(sensor);
            }
            return mainNodes;
        }
    }
}
