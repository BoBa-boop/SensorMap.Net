using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Commands.SensorCommands;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using SensorMap.View;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using DataFormats = System.Windows.DataFormats;
using DataObject = System.Windows.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;
using MessageBox = System.Windows.MessageBox;

namespace SensorMap.ViewModel
{
    /// <summary>
    /// ПУСТОЙ Mechanisms у секторов
    /// </summary>
    public class MechanismVM : ReactiveObject
    {
        private IAppDbContextFactory _appDbContextFactory;
        private readonly IDataBaseProvider _provider;
        private readonly IDataService _service;
        private Sector? currentSector; 
        private Mechanism? currentMech;
        private Sensor? _curSensor;
        private bool isEditMode;
        private bool _isShowSensors;
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
        [Reactive] public bool CanUndo => _undoRedoManager.CanUndo;
        [Reactive] public bool CanRedo => _undoRedoManager.CanRedo;
        [Reactive] public bool IsShowSensors
        {
            get => _isShowSensors;
            set
            {
                _isShowSensors = value; this.RaiseAndSetIfChanged(ref _isShowSensors, value);
            }
        }
        [Reactive] public ObservableCollection<Sector> Sectors { get; set; } = new();
        [Reactive] public ObservableCollection<PLC> PLCs { get; set; } = new();
        [Reactive] public TreeViewCollection<SensorType, Sensor> Sensors { get; set; }
        [Reactive] public ObservableCollection<Sensor> SensorList { get; set; }
        public MechanismVM(IDataBaseProvider provider, IDataService service, INavigation _nav,IAppDbContextFactory appDbContextFactory)
        {
            Navigation = _nav;
            _provider = provider;
            _service = service;
            _appDbContextFactory = appDbContextFactory; 
            CurrentSector = _service.CurrentSector_Global;
            CurrentMech = _service.CurrentMechanism_Global;
            using (var _dbContext = _appDbContextFactory.CreateDbContext())
            {
                GetDataFromDB(_dbContext);
                Func<SensorType, Sensor, bool> filter = (type, sensor) => sensor.SensorTypeID == type.Id;
                Sensors = new TreeViewCollection<SensorType, Sensor>("Name", sensorTypes, SensorList, filter);
            }


            NavigateToSectors = new RelayCommand(() => Navigation.NavigateTo<SectorsVM>());
            
            AddSensorToMap = new RelayCommand<object>((obj) =>
            {
                if (obj is Sensor sensor)
                {
                    //Пакет данных нового датчика
                    SensorAssignments sensorAssignments = new SensorAssignments()
                    {
                        SensorId = sensor.Id,
                        Sensor = sensor,
                        MechanismId = CurrentMech!.Id,
                        Mechanism = CurrentMech,
                        Width = 30,
                        Height = 30
                    };
                    //Добавление в коллекцию. Далее обработка события и добавления визуального элемента
                    CurrentMech.SensorsAssig!.Add(sensorAssignments);
                    
                }
                if(obj is AddSensor command)
                {
                    //Выполнение команды "Добавить"
                    _undoRedoManager.Do(command);
                }
            }, (obj) =>
            { 
                if (obj is Sensor sensor)
                    return CanExecuteAddSensor(sensor);
                return false; 
            });
            RemoveSensorCommand = new RelayCommand<object[]>((obj) => 
            {
                if (obj[0] is RemoveSensor command && obj[1] is SensorAssignments sensor)
                {
                    using (var dbContext = _appDbContextFactory.CreateDbContext())
                    {
                        var element = dbContext.SensorAssignments.Find(sensor.Id);
                        if (element != null)
                            dbContext.SensorAssignments.Entry(element).State = EntityState.Deleted;
                        dbContext.SaveChangesAsync();
                    }
                    _undoRedoManager.Do(command);
                }
            });
            DragSensorCommand = new RelayCommand<object>((obj) =>
            {
                if(obj is TextBlock tb)
                    if (tb.DataContext is TreeNode<Sensor> node)
                    {
                            SensorAssignments sensorAssignments = new SensorAssignments()
                            {
                                SensorId = node.Data.Id,
                                Sensor = node.Data,
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
            SaveSensorPlace = new RelayCommand(SaveCoordinates);
            ShowSensorMechanism = new RelayCommand<Mechanism>((obj) =>
            {
                if (obj is Mechanism mechanism)
                {
                    MechanismSensorsWindow window = new MechanismSensorsWindow();
                    MechSensorsVM mechSensorsVM = new MechSensorsVM(_appDbContextFactory, mechanism);
                    window.DataContext = mechSensorsVM;
                    window.Show();
                    window.Loaded += (s,e) => IsShowSensors = true;
                    window.Closed += (s, e) => IsShowSensors = false;

                }
            }, (obj) => 
            { 
                if (obj==null || obj is Mechanism mech && mech.SensorsAssig.Count() == 0) return false; 
                return true;
            });


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

        private async void GetDataFromDB(EF.AppDBContext _dbContext)
        {
            var querySector = await _dbContext.Sectors.AsNoTracking().Include(x => x.Mechanisms)
                                                      .ThenInclude(x => x.SensorsAssig)
                                                      .ThenInclude(x => x.Sensor).ThenInclude(x => x.SensorType).ToListAsync();
            var queryTypes = await _dbContext.SensorTypes.AsNoTracking().ToListAsync();
            var queryPLC = await _dbContext.PLCs.AsNoTracking().ToListAsync();
            var querySensors = await _dbContext.Sensors.AsNoTracking().Include(x=>x.SensorType).ToListAsync();
            
            
            sensorTypes = new ObservableCollection<SensorType>(queryTypes);
            Sectors = new ObservableCollection<Sector>(querySector);
            SensorList = new ObservableCollection<Sensor>(querySensors);
            PLCs = new ObservableCollection<PLC>(queryPLC);
        }

        private bool CanExecuteAddSensor(object selectedSensor)
        {
            if (selectedSensor == null) return false;
            if (CurrentMech == null||CurrentMech.Image==null)
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
            else if (CurrentMech.PLCID == 0)
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
            using (var dbContext = _appDbContextFactory.CreateDbContext())
            {
                foreach (var sa in CurrentMech.SensorsAssig)
                {
                    if(sa.Id == 0)
                        dbContext.Entry(sa).State = EntityState.Added;
                    else
                    {
                        dbContext.Entry(sa).State = EntityState.Modified;
                    }
                }
                dbContext.SaveChangesAsync();

                Growl.Success("Данные сохранены");
            }
        }
        public ICommand SaveSensorPlace { get; }
        public ICommand ShowSensorMechanism { get; }
        public ICommand NavigateToSectors { get; }

        /// <summary>
        /// Команда добавления датчика на карту. Формируются данные нового датчика, добавляются в коллекцию
        /// и обрабатывает событие добавления для создания визуального CustomSensor
        /// </summary>
        public ICommand AddSensorToMap { get; }

        /// <summary>
        /// Команда удаления датчика. Команда приходит от CustomSensor в SensorDragDrop.
        /// Происходит удаление из коллекции и Canvas
        /// </summary>
        public ICommand RemoveSensorCommand { get; }
        public ICommand TransformSensorCommand { get; }
        public ICommand DragSensorCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ObservableCollection<SensorType> sensorTypes { get; private set; }
    }
}
