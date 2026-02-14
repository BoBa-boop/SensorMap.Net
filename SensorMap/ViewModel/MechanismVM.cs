using CommunityToolkit.Mvvm.Input;
using DynamicData;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Expression.Shapes;
using HandyControl.Tools;
using HandyControl.Tools.Extension;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Commands.SensorCommands;
using SensorMap.CustomControls;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using SensorMap.View;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using DataFormats = System.Windows.DataFormats;
using DataObject = System.Windows.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;

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
        private Mechanism? currentMech;
        private ObservableCollection<SensorType> sensorTypes { get; set; }
        public readonly UndoRedoStack _undoRedoManager = new UndoRedoStack();

        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }
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
        [Reactive] public bool CanUndo => _undoRedoManager.CanUndo;
        [Reactive] public bool CanRedo => _undoRedoManager.CanRedo;

        [Reactive] public Mechanism? CurrentMech
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
        private bool isEditMode;
        [Reactive] public Sensor? CurrentSensor
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
        [Reactive] public ObservableCollection<Sector> Sectors { get; set; } = new();
        [Reactive] public ObservableCollection<PLC> PLCs { get; set; } = new();
        [Reactive] public TreeViewCollection<SensorType, Sensor> Sensors { get; set; }
        [Reactive] public ObservableCollection<Mechanism> Mechanisms { get; set; } = new();
        public MechanismVM(IDataBaseProvider provider, IDataService service, INavigation _nav)
        {
            Navigation = _nav;
            _provider = provider;
            _service = service;
            
            sensorTypes = _service.SensorTypes;
            CurrentSector = _service.CurrentSector_Global;
            CurrentMech = _service.CurrentMechanism_Global;
            PLCs = _service.PLCs;
            Sectors = _service.Sectors;
            Func<SensorType, Sensor, bool> filter = (type, sensor) => sensor.SensorTypeID == type.Id;
            Sensors = new TreeViewCollection<SensorType, Sensor>("Name", sensorTypes, _service.Sensors, filter);
            NavigateToSectors = new RelayCommand(() => Navigation.NavigateTo<SectorsVM>());
            Mechanisms = new(_dbContext.Mechanisms.Where(x => x.Sector != null && x.Sector.Id == (CurrentSector.Id)).Include(x => x.SensorsAssig).ToList());
            AddSensorToMap = new RelayCommand<object>((obj) =>
            {
                if (obj is Sensor sensor)
                {
                    SensorAssignments sensorAssignments = new SensorAssignments()
                    {
                        SensorId = sensor.Id,
                        Sensor = sensor,
                        MechanismId = CurrentMech!.Id,
                        Mechanism = CurrentMech,
                        Width = 30,
                        Height = 30
                    };
                    CurrentMech.SensorsAssig!.Add(sensorAssignments);
                }
                if(obj is AddSensor command)
                {
                    _undoRedoManager.Do(command);
                }
            }, (obj) =>
            { 
                if (obj is Sensor sensor)
                    return CanExecuteAddSensor(sensor);
                return false; 
            });
            DragSensorCommand = new RelayCommand<object>((obj) =>
            {
                if(obj is TextBlock tb)
                if (tb.DataContext is TreeNode<Sensor> node)
                {
                    SensorAssignments sensorAssignments = new SensorAssignments()
                    {
                        SensorId = node.Data.Id,
                        Sensor = _service.Sensors.FirstOrDefault(x=>x.Id== node.Data.Id),
                        MechanismId = CurrentMech.Id,
                        Mechanism = CurrentMech,
                        Width = 30,
                        Height = 30
                    };

                    DragDrop.DoDragDrop(obj as TextBlock, new DataObject(DataFormats.Serializable, sensorAssignments), DragDropEffects.Copy);
                }
            }, (obj) => 
            {
                if (obj is TextBlock tb)
                    if (tb.DataContext is TreeNode<Sensor> node)
                        return CanExecuteAddSensor(node);
                return false;
            });
            SaveSensorPlace = new RelayCommand<Mechanism>((m) => SaveCoordinates());
            _service.WhenAnyValue(x => x.IsEditMode)
               .BindTo(this, x => x.IsEditMode);
            _undoRedoManager.WhenAnyValue(x => x.CanUndo)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(CanUndo)));

            _undoRedoManager.WhenAnyValue(x => x.CanRedo)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(CanRedo)));
            UndoCommand = new RelayCommand(()=> _undoRedoManager.Undo());
            RedoCommand = new RelayCommand(() => _undoRedoManager.Redo());
            TransformSensorCommand = new RelayCommand<object>((obj) => { if (obj is TransformationSensor command) _undoRedoManager.Do(command); });

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
            else if (CurrentMech.PLC == null)
            {
                Growl.Error(new GrowlInfo
                {
                    Message = $"Необходимо добавить PLC к механизации",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    Type = InfoType.Error,
                    WaitTime = 2
                });
                return false;
            }
            else return true;
        }

        private void SaveCoordinates()
        {//user работает с картой. У него заполняется стэк Undo. Когда он уходит с рабочей вкладки и у него отсутсвует флаг сохранения, необходимо 
         //выдавать предупреждение о не сохраненных данных. Здесь будет даваться флаг, но когда происходит новое изменение стэка флаг сбрасывается
         //разобраться с сохранением
            //_provider.AddSensorsAssignmentAsync(CurrentMech!.SensorsAssig!);
        }
        public ICommand SaveSensorPlace { get; }
        public ICommand NavigateToSectors { get; }
        public ICommand AddSensorToMap { get; }
        public ICommand TransformSensorCommand { get; }
        public ICommand DragSensorCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
    }
}
