using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Model.TreeNode;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    /// <summary>
    /// ПУСТОЙ Mechanisms у секторов
    /// </summary>
    public class MechanismVM : ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        private readonly IDataService _service;
        private Sector? currentSector;
        private ObservableCollection<SensorType> sensorTypes { get; set; }
        [Reactive] public Sector? CurrentSector
        {
            get => currentSector;
            set
            {
                if (value != null)
                {
                    this.RaiseAndSetIfChanged(ref currentSector, value);
                    currentSector = value;
                }
            }
        }
        private Mechanism? currentMech;

        [Reactive]
        public Mechanism? CurrentMech
        {
            get => currentMech;
            set
            {
                if (value != null)
                {
                    this.RaiseAndSetIfChanged(ref currentMech, value);
                    currentMech = value;
                }
            }
        }
        private Sensor? _curSensor;

        [Reactive]
        public Sensor? CurrentSensor
        {
            get => _curSensor;
            set
            {
                if (value != null)
                {
                    this.RaiseAndSetIfChanged(ref _curSensor, value);
                    _curSensor = value;
                }
            }
        }
        private double _x;
        [Reactive]
        public double X
        {
            get => _x;
            set
            {
                this.RaiseAndSetIfChanged(ref _x, value);
                _x = value;
            }
        }
        private double _y;
        [Reactive]
        public double Y
        {
            get => _y;
            set
            {
                this.RaiseAndSetIfChanged(ref _y, value);
                _y = value;
            }
        }
        [Reactive] public ObservableCollection<Sector> Sectors { get; set; } = new();
        [Reactive] public ObservableCollection<SensorsTreeNode> Sensors { get; set; } = new();
        [Reactive] public ObservableCollection<Mechanism> Mechanisms { get; set; } = new();
        public MechanismVM(IDataBaseProvider provider, IDataService service, Sector sector = null)
        {
            _provider = provider;
            _service = service;
            sensorTypes = _service.SensorTypes;
            CurrentSector = sector;
            Sectors = _service.Sectors;
            Sensors = new ObservableCollection<SensorsTreeNode>(TreeNodeSensors());

            Mechanisms = new(_service.Mechanisms.Where(x => x.Sector != null && x.Sector.Id == (CurrentSector?.Id ?? 0)).ToList());

            SaveSensorPlace = new RelayCommand(() => MessageBox.Show($"X:{X};Y:{Y}"));
            this.WhenAnyValue(x=>x.CurrentSensor).ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe((obj) => { if (obj != null) MessageBox.Show($"{obj}"); });
        }
        private IEnumerable<SensorsTreeNode> TreeNodeSensors()
        {
            var mainNodes = new ObservableCollection<SensorsTreeNode>();
            var types = sensorTypes;
            foreach (var type in types)
            {
                mainNodes.Add(new SensorsTreeNode()
                {
                    Name = type.Name,
                    Image = type.Image
                });
            }
            foreach (var sensor in _service.Sensors)
            {
                var node = mainNodes.FirstOrDefault(nodes => nodes.Name == sensor.SensorType.Name.ToString());
                node?.Children.Add(sensor);
            }
            return mainNodes;
        }
        public ICommand SaveLayout { get; set; }
        public ICommand SaveSensorPlace { get; }
    }
}
