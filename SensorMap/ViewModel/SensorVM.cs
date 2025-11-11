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
            GetInfoAboutType = new RelayCommand<object>((s) =>
            {
                ShowPopup(s);
            });
        }

        private static void ShowPopup(object sender)
        {
            var button = sender as FrameworkElement;
            var picker = SingleOpenHelper.CreateControl<ColorPicker>();
            var window = new PopupWindow
            {
                PopupElement = picker,
                WindowStartupLocation= WindowStartupLocation.CenterOwner
            };
            picker.SelectedColorChanged += delegate { window.Close(); };
            picker.Canceled += delegate { window.Close(); };
            window.Show(button,false);
        }

        public ICommand GetInfoAboutType { get; }
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
                var node = mainNodes.FirstOrDefault(nodes => nodes.Name == sensor.Type.ToString());
                node?.Children.Add(sensor);
            }
            return mainNodes;
        }
    }
}
