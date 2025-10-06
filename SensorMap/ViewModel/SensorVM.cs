using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Model.TreeNode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using static SensorMap.Model.Sensor;

namespace SensorMap.ViewModel
{
    public class SensorVM:ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        private Sensor _sensorsTreeNode;
        [Reactive]public Sensor SelectedNode
        {
            get => _sensorsTreeNode;
            set => this.RaiseAndSetIfChanged(ref _sensorsTreeNode, value);
        }
        [Reactive]public ObservableCollection<SensorsTreeNode> Sensors {  get; set; }
        public SensorVM(IDataBaseProvider provider)
        {
            _provider = provider;
            Sensors = new ObservableCollection<SensorsTreeNode>(TreeNodeSensors());
        }
        private IEnumerable<SensorsTreeNode> TreeNodeSensors()
        {
            var mainNodes = new ObservableCollection<SensorsTreeNode>();
            var sensors = new ObservableCollection<Sensor>(_provider.GetAllSensors());
            var types = Enum.GetValues<SensorType>();
            foreach (var type in types)
            {
                mainNodes.Add(new SensorsTreeNode()
                {
                    Name = type.ToString()
                });
            }
            foreach (var sensor in sensors)
            {
                var node = mainNodes.FirstOrDefault(nodes => nodes.Name == sensor.Type.ToString());
                node?.Children.Add(sensor);
            }
            return mainNodes;
        }
    }
}
