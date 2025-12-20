using CommunityToolkit.Mvvm.Input;
using DynamicData;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Expression.Shapes;
using HandyControl.Tools;
using HandyControl.Tools.Extension;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Model.TreeNode;
using SensorMap.View;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
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
        public UndoRedoStack UndoRedoStack { get; set; }

        [Reactive] public INavigation? Navigation { get; set; }
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
        private bool _CanUndo;
        private bool _CanRedo;
        [Reactive]
        public bool CanUndo
        {
            get => _CanUndo;
            set 
            {
                this.RaiseAndSetIfChanged(ref _CanUndo, value);
            }
        }

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
        
        [Reactive] public ObservableCollection<SensorAssignments> CanvasSensors { get; set; }
        [Reactive] public ObservableCollection<Sector> Sectors { get; set; } = new();
        [Reactive] public ObservableCollection<SensorsTreeNode> Sensors { get; set; } = new();
        [Reactive] public ObservableCollection<Mechanism> Mechanisms { get; set; } = new();
        public MechanismVM(IDataBaseProvider provider, IDataService service, INavigation _nav, Sector sector = null)
        {
            Navigation = _nav;
            _provider = provider;
            _service = service;
            UndoRedoStack = new UndoRedoStack();
            sensorTypes = _service.SensorTypes;
            CanvasSensors = new();
            CurrentSector = sector;
            Sectors = _service.Sectors;
            Sensors = new ObservableCollection<SensorsTreeNode>(TreeNodeSensors());
            NavigateToSectors = new RelayCommand(() => Navigation.NavigateTo<SectorsVM>());
            Mechanisms = new(_service.Mechanisms.Where(x => x.Sector != null && x.Sector.Id == (CurrentSector?.Id ?? 0)).ToList());
            AddSensorToMap = new RelayCommand<object>((obj) =>
            {
                if (obj is Sensor sensor)
                {
                    SensorAssignments sensorAssignments = new SensorAssignments()
                    {
                        SensorId = sensor.Id,
                        Sensor = sensor,
                        MechanismId = CurrentMech.Id,
                        PLCId = CurrentMech.PLCID,
                        Mechanism = CurrentMech,
                        PLC = CurrentMech.PLC
                    };
                    CanvasSensors.Add(sensorAssignments);
                }
            }, (j) => CanExecuteAddSensor(j));
            DragSensorCommand = new RelayCommand<object>((obj) =>
            {
                var tb = obj as TextBlock;
                if (tb.DataContext is Sensor sensor)
                {
                    SensorAssignments sensorAssignments = new SensorAssignments()
                    {
                        SensorId = sensor.Id,
                        Sensor = sensor,
                        MechanismId = CurrentMech.Id,
                        PLCId = CurrentMech.PLCID,
                        Mechanism = CurrentMech,
                        PLC = CurrentMech.PLC
                    };

                    DragDrop.DoDragDrop(obj as TextBlock, new DataObject(DataFormats.Serializable, sensorAssignments), DragDropEffects.Copy);
                }
            }, (j) => CanExecuteAddSensor(j));
            SaveSensorPlace = new RelayCommand(() => SaveCoordinates());
            this.WhenAnyValue(x => x.CurrentMech).Subscribe((obj) =>
            {
                if (CurrentMech != null)
                {
                    CanvasSensors.Clear();
                    if(CurrentMech.SensorsAssig!=null)
                        CanvasSensors.AddRange(CurrentMech.SensorsAssig);
                }
            });
            this.WhenAnyValue(x => x.UndoRedoStack.UndoCount).ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe((count) => CanUndo = count > 0 );
            UndoCommand = new RelayCommand(()=> UndoRedoStack.Undo());
            RedoCommand = new RelayCommand(() => UndoRedoStack.Redo());

        }

        private bool CanExecuteAddSensor(object selectedSensor)
        {
            if (selectedSensor == null) return false;
            if (CurrentMech == null)
            {
                Growl.Error(new GrowlInfo
                {
                    Message = $"Необходимо выбрать механизацию!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    Type = InfoType.Error,
                    WaitTime = 2
                });
                return false;
            }
            //else if (CurrentMech.PLC == null)
            //{
            //    Growl.Error(new GrowlInfo
            //    {
            //        Message = $"Необходимо добавить PLC к механизации",
            //        CancelStr = "Ignore",
            //        ShowDateTime = false,
            //        Type = InfoType.Error,
            //        WaitTime = 2
            //    });
            //    return false;
            //}
            else return true;
        }

        private void SaveCoordinates()
        {//user работает с картой. У него заполняется стэк Undo. Когда он уходит с рабочей вкладки и у него отсутсвует флаг сохранения, необходимо 
            //выдавать предупреждение о не сохраненных данных. Здесь будет даваться флаг, но когда происходит новое изменение стэка флаг сбрасывается
            //разобраться с сохранением
            foreach (var obj in CanvasSensors)
            {
                if (obj.Id == 0)
                {
                    _provider.AddSensorAssignmentAsync(obj);
                }
                else
                {
                    _provider.Update<SensorAssignments>(obj);
                }
            }
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
        public ICommand NavigateToSectors { get; }
        public ICommand AddSensorToMap { get; }
        public ICommand DragSensorCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
    }
}
