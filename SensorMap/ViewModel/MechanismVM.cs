using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Expression.Shapes;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Commands.SensorCommands;
using SensorMap.CustomControls;
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
        private ITempImage _imgControl;
        private readonly IFileManagment _fileManagment;
        private Sector? currentSector; 
        private Mechanism? currentMech;
        private Sensor _curSensor;
        private bool isEditMode;
        private bool _isShowSensors;
        private bool _hasChanges;
        private SensorAssignments _selectSensor;
        private readonly Dictionary<int, UndoRedoStack> _undoRedoStacks = new();
        private IDisposable? _undoSub;
        private IDisposable? _redoSub;
        private UndoRedoStack CurrentStack
        {
            get
            {
                if (CurrentMech == null) return null!;
                if (!_undoRedoStacks.ContainsKey(CurrentMech.Id))
                    _undoRedoStacks[CurrentMech.Id] = new UndoRedoStack();
                return _undoRedoStacks[CurrentMech.Id];
            }
        }

        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }
        [Reactive] public INavigation? Navigation { get; set; }
        /// <summary>
        /// Переменная хранит значение из TreeView выбранного участка
        /// </summary>
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
        /// <summary>
         /// Переменная хранит значение из TreeView выбранной механизации
         /// </summary>
        [Reactive] public Mechanism? CurrentMech
        {
            get => currentMech;
            set
            {
                if (value != null)
                {
                    this.RaiseAndSetIfChanged(ref currentMech, value);
                    currentMech = value;
                    SubscribeToCurrentStack();
                }
            }
        }
        /// <summary>
         /// Переменная хранит значение из TreeView выбранного датчика
         /// </summary>
        [Reactive] public Sensor CurrentSensor
        {
            get => _curSensor;
            set
            {
                this.RaiseAndSetIfChanged(ref _curSensor, value);
            }
        }
        
        /// <summary>
        /// Датчик выбранный из списка MechSensors
        /// </summary>
        [Reactive]
        public SensorAssignments SelectedSensor
        {
            get { return _selectSensor; }
            set { this.RaiseAndSetIfChanged(ref _selectSensor, value); }
        }

        [Reactive] public bool CanUndo => CurrentStack?.CanUndo ?? false;
        [Reactive] public bool CanRedo => CurrentStack?.CanRedo ?? false;
        [Reactive] public bool IsShowSensors
        {
            get => _isShowSensors;
            set
            {
                _isShowSensors = value; this.RaiseAndSetIfChanged(ref _isShowSensors, value);
            }
        }
        [Reactive] public bool HasChanges
        {
            get { return _hasChanges; }
            set
            {
                this.RaiseAndSetIfChanged(ref _hasChanges, value);
            }
        }
        [Reactive] public ObservableCollection<Sector>? Sectors { get; set; } = new();
        [Reactive] public ObservableCollection<SensorType>? sensorTypes { get; private set; }
        [Reactive] public ObservableCollection<Device> Devices { get; set; } = new();
        [Reactive] public TreeViewCollection<SensorType, Sensor>? Sensors { get; set; }
        [Reactive] public ObservableCollection<Sensor>? SensorList { get; set; }
        public MechanismVM(IDataBaseProvider provider, IDataService service, INavigation _nav,
            IAppDbContextFactory appDbContextFactory, ITempImage imageControl,
            IFileManagment fileManagment,
            Mechanism curMechanism = null )
        {
            Navigation = _nav;
            _provider = provider;
            _service = service;
            _imgControl = imageControl;
            _appDbContextFactory = appDbContextFactory;
            _fileManagment = fileManagment;
            using (var _dbContext = _appDbContextFactory.CreateDbContext())
            {
                GetDataFromDB(_dbContext);
            }
            var transferDataSector = curMechanism?.SectorID ?? _service.CurrentSector_Global?.Id;
            CurrentSector = Sectors.Where(x => x.Id == transferDataSector).FirstOrDefault();
            if (curMechanism!=null && CurrentSector!=null)
            {
                
                CurrentMech = CurrentSector?.Mechanisms?.Where(x => x.Id == curMechanism.Id).FirstOrDefault();
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
                if (obj is AddSensor command)
                {
                    //Выполнение команды "Добавить"
                    CurrentStack?.Do(command);
                }
            }, (obj) =>
            { 
                if (obj is Sensor sensor)
                    return CanExecuteAddSensor(sensor);
                return false; 
            });
            DeleteSensorCommand = new RelayCommand<object[]>((obj) => 
            {
                if (obj[0] is RemoveSensor command && obj[1] is List<CustomSensor> sensors)
                {
                    CurrentStack?.Do(command);
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
            ShowSensorMechanism = new RelayCommand(()=>
            {
                if (CurrentMech!=null)
                {
                    MechanismSensorsWindow window = new MechanismSensorsWindow();
                    MechSensorsVM mechSensorsVM = new MechSensorsVM(imageControl, CurrentMech,SensorList);
                    window.DataContext = mechSensorsVM;
                    mechSensorsVM.IsEditMode = IsEditMode;
                    mechSensorsVM.WhenAnyValue(x => x.SelectedSensor).BindTo(this,x=>x.SelectedSensor);
                    window.ShowDialog();
                    if(mechSensorsVM.HasChanges) HasChanges=true;

                }
            });
            ShowScheme = new RelayCommand<object>((obj) =>
            {
                if (obj is Mechanism mech)
                {
                    _fileManagment.OpenFileInExplorer(mech.Files.First().NameFile);
                }
            },(obj) => 
            {
                if (obj == null) return false;
                var mech = obj as Mechanism;
                bool SensorsNotNull = mech!.SensorsAssig != null && mech.SensorsAssig.Any();
                bool FilesNotNull = mech.Files != null && mech.Files.Any();
                if (SensorsNotNull && FilesNotNull) return true;
                return false;
            });

                _service.WhenAnyValue(x => x.IsEditMode)
               .BindTo(this, x => x.IsEditMode);

            UndoCommand = new RelayCommand(() => CurrentStack?.Undo());
            RedoCommand = new RelayCommand(() => CurrentStack?.Redo());
            TransformSensorCommand = new RelayCommand<object>((obj) => 
            {
                if (obj is TransformationSensors command)
                {
                    CurrentStack?.Do(command);
                }
            });

        }

        private void SubscribeToCurrentStack()
        {
            _undoSub?.Dispose();
            _redoSub?.Dispose();
            var stack = CurrentStack;
            if (stack == null) return;
            _undoSub = stack.WhenAnyValue(x => x.CanUndo)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(CanUndo));
                    HasChanges = CanUndo;
                });
            _redoSub = stack.WhenAnyValue(x => x.CanRedo)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(CanRedo)));
            this.RaisePropertyChanged(nameof(CanUndo));
            this.RaisePropertyChanged(nameof(CanRedo));
        }

        private async void GetDataFromDB(EF.AppDBContext _dbContext)
        {
            var querySector = await _dbContext.Sectors.ToListAsync();
            var queryMech = await _dbContext.Mechanisms.Include(m => m.Files).Include(m=>m.SensorsAssig).AsSplitQuery().ToListAsync();
            var queryTypes = await _dbContext.SensorTypes.ToListAsync();
            var queryDevice = await _dbContext.Devices.ToListAsync();
            var querySensors = await _dbContext.Sensors.Include(x=>x.SensorType).ToListAsync();
            
            
            sensorTypes = new ObservableCollection<SensorType>(queryTypes);
            SensorList = new ObservableCollection<Sensor>(querySensors);
            Devices = new ObservableCollection<Device>(queryDevice);
            Func<SensorType, Sensor, bool> filter = (type, sensor) => sensor.SensorTypeID == type.Id;
            Sensors = new TreeViewCollection<SensorType, Sensor>("Name", sensorTypes, SensorList, filter);
            Sectors = new ObservableCollection<Sector>(querySector);
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
            else if (CurrentMech.DeviceID == 0)
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
                    if (!IsValidData(sa)) break;
                    if(sa.Id == 0||sa.IsNew)
                    {
                        //var tempSensor = (SensorAssignments)sa.Clone();//клонирую, потому что при Id = 0 не получиться обнаружит датчик при undo TransforCommand
                        //tempSensor.Id = 0;
                        dbContext.Entry(sa).State = EntityState.Added;
                        sa.IsNew = false;
                    }
                    else if (sa.ToDelete)
                    {
                        dbContext.Entry(sa).State = EntityState.Deleted;
                    }
                    else if(sa.IsModified)
                    {
                        var original = _service.GetOriginalEntry(dbContext, sa);
                        if (original != null && dbContext.ChangeTracker.HasChanges())
                            dbContext.Entry(original).CurrentValues.SetValues(sa);
                        else
                        {
                            if (original != null)
                                dbContext.Entry(original).State = EntityState.Detached;
                            dbContext.Update(sa);
                        }
                        //dbContext.Entry(sa).State = EntityState.Modified;
                    }
                    sa.IsModified = false;
                }
                dbContext.SaveChangesAsync();                
                HasChanges = false;
                Growl.Success("Данные сохранены");
            }
        }

        private bool IsValidData(SensorAssignments sensor)
        {
            if(sensor.Width <=10 || sensor.Height <=10) return false;
            if(sensor.X <=0 || sensor.Y <=0) return false;
            return true;
        }
        public ICommand SaveSensorPlace { get; }
        public ICommand ShowSensorMechanism { get; }
        public ICommand ShowScheme { get; }
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
        public ICommand DeleteSensorCommand { get; }
        public ICommand TransformSensorCommand { get; }
        public ICommand DragSensorCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand SetCurrentSensorCommand { get; }
    }
}
