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
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private MapObject _selectMapObject;
        private readonly Dictionary<int, UndoRedoStack> _undoRedoStacks = new();
        private IDisposable? _undoSub;
        private IDisposable? _redoSub;
        private Sensor _curDevice;
        private MapObject _selectMapObject;

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
                    SubscribeToCurrentStack();
                }
            }
        }
        [Reactive] public Sensor CurrentSensor
        {
            get => _curSensor;
            set
            {
                this.RaiseAndSetIfChanged(ref _curSensor, value);
            }
        }
        [Reactive]
        public Sensor CurrentDevice
        {
            get => _curDevice;
            set
            {
                this.RaiseAndSetIfChanged(ref _curDevice, value);
            }
        }
        /// <summary>
        /// Объект (датчик или устройство) выбранный на карте или из списка
        /// </summary>
        [Reactive]
        public MapObject SelectedMapObject
        {
            get { return _selectMapObject; }
            set { this.RaiseAndSetIfChanged(ref _selectMapObject, value); }
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
        [Reactive] public TreeViewCollection<DeviceType, Device> Devices { get; set; }
        [Reactive] public TreeViewCollection<SensorType, Sensor>? Sensors { get; set; }
        [Reactive] private ObservableCollection<Sensor>? SensorsList { get; set; }
        [Reactive] private ObservableCollection<Device>? DevicesList { get; set; }
        public MechanismVM(IDataBaseProvider provider, IDataService service, INavigation _nav,
            IAppDbContextFactory appDbContextFactory, ITempImage imageControl,
            IFileManagment fileManagment,
            Mechanism curMechanism = null)
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
            if (curMechanism != null && CurrentSector != null)
            {
                CurrentMech = CurrentSector?.Mechanisms?.Where(x => x.Id == curMechanism.Id).FirstOrDefault();
            }

            NavigateToSectors = new RelayCommand(() => Navigation.NavigateTo<SectorsVM>());

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
                    CurrentMech.MapObjects!.Add(sensorAssignments);
                }
                if (obj is Device device)
                {
                    DeviceAssignment deviceAssignment = new DeviceAssignment()
                    {
                        DeviceId = device.Id,
                        Device = device,
                        MechanismId = CurrentMech!.Id,
                        Mechanism = CurrentMech,
                        Width = 50,
                        Height = 30,
                        Description = device.Name
                    };
                    CurrentMech.MapObjects!.Add(deviceAssignment);
                }
                if (obj is AddSensor command)
                {
                    CurrentStack?.Do(command);
                }
            }, (obj) =>
            {
                if (obj is Sensor sensor)
                    return CanExecuteAddSensor(sensor);
                if (obj is Device device)
                    return CanExecuteAddDevice(device);
                return false;
            });

            DeleteSensorCommand = new RelayCommand<object[]>((obj) =>
            {
                if (obj[0] is RemoveSensor command && obj[1] is List<IMapElement> objects)
                {
                    CurrentStack?.Do(command);
                }
            });

            DragSensorCommand = new RelayCommand<object>((obj) =>
            {
                if (obj is TextBlock tb)
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
            DragDeviceCommand = new RelayCommand<object>((obj) =>
            {
                if (obj is TextBlock tb)
                    if (tb.DataContext is TreeNode<Device> device)
                    {
                        DeviceAssignment deviceAssignment = new DeviceAssignment()
                        {
                            DeviceId = device.Data.Id,
                            Device = device.Data,
                            MechanismId = CurrentMech.Id,
                            Mechanism = CurrentMech,
                            Width = 50,
                            Height = 30,
                            Description = device.Name
                        };
                        DragDrop.DoDragDrop(obj as TextBlock, new DataObject(DataFormats.Serializable, deviceAssignment), DragDropEffects.Copy);
                    }
            }, (obj) =>
            {
                if (obj is TextBlock tb)
                    if (tb.DataContext is TreeNode<Device> device)
                        return CanExecuteAddDevice(device);
                return false;
            });
            SaveSensorPlace = new RelayCommand(SaveCoordinates);
            ShowSensorMechanism = new RelayCommand(() =>
            {
                if (CurrentMech != null)
                {
                    MechanismSensorsWindow window = new MechanismSensorsWindow();
                    MechSensorsVM mechSensorsVM = new MechSensorsVM(imageControl,CurrentMech,SensorsList,DevicesList);
                    window.DataContext = mechSensorsVM;
                    mechSensorsVM.IsEditMode = IsEditMode;
                    mechSensorsVM.WhenAnyValue(x => x.SelectedMapObject).BindTo(this, x => x.SelectedMapObject);
                    window.ShowDialog();
                    if (mechSensorsVM.HasChanges) HasChanges = true;
                }
            });
            ShowScheme = new RelayCommand<object>((obj) =>
            {
                if (obj is Mechanism mech)
                {
                    _fileManagment.OpenFileInExplorer(mech.Files.First().NameFile);
                }
            }, (obj) =>
            {
                if (obj == null) return false;
                var mech = obj as Mechanism;
                bool FilesNotNull = mech.Files != null && mech.Files.Any();
                if (FilesNotNull) return true;
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
            //var queryMech = await _dbContext.Mechanisms
            //    .Include(m => m.Files)
            //    .Include(m => m.MapObjects)
            //        .ThenInclude(o => ((SensorAssignments)o).Sensor)
            //            .ThenInclude(s => s!.SensorType)
            //    .Include(m => m.MapObjects)
            //        .ThenInclude(o => ((DeviceAssignment)o).Device)
            //    .AsSplitQuery().ToListAsync();
            var querySector = await _dbContext.Sectors.ToListAsync();
            var queryMech = await _dbContext.Mechanisms.Include(m => m.Files)
                .Include(m=>m.MapObjects).AsSplitQuery().ToListAsync();
            var queryTypes = await _dbContext.SensorTypes.ToListAsync();
            var queryDevTypes = await _dbContext.DeviceTypes.ToListAsync();
            var queryDevice = await _dbContext.Devices.ToListAsync();
            var querySensors = await _dbContext.Sensors.Include(x=>x.SensorType).ToListAsync();
            var queryDevTypes = await _dbContext.DeviceTypes.ToListAsync();

            sensorTypes = new ObservableCollection<SensorType>(queryTypes);
            SensorsList = new ObservableCollection<Sensor>(querySensors);
            DevicesList = new ObservableCollection<Device>(queryDevice);

            Func<SensorType, Sensor, bool> filter = (type, sensor) => sensor.SensorTypeID == type.Id;
            Func<DeviceType, Device, bool> filter2 = (type, devce) => devce.DeviceTypeId == type.Id;
            Sensors = new TreeViewCollection<SensorType, Sensor>("Name", sensorTypes, SensorsList, filter);
            Devices = new TreeViewCollection<DeviceType, Device>("Name", new(queryDevTypes), new(queryDevice), filter2);
            Devices = new TreeViewCollection<DeviceType,Device>("Name", new(queryDevTypes),new(queryDevice),filter2);
            Sectors = new ObservableCollection<Sector>(querySector);
        }

        private bool CanExecuteAddSensor(object selectedSensor)
        {
            if (selectedSensor == null) return false;
            if (CurrentMech == null || CurrentMech.Image == null)
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
        private bool CanExecuteAddDevice(object selectedDevice)
        {
            if (selectedDevice == null) return false;
            if (CurrentMech == null || CurrentMech.Image == null)
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
            return true;
        }
        private async void SaveCoordinates()
        {
            using (var dbContext = _appDbContextFactory.CreateDbContext())
            {
                if (CurrentMech?.MapObjects == null) return;

                var existingMech = await dbContext.Mechanisms
                    .Include(m => m.MapObjects)
                    .FirstOrDefaultAsync(m => m.Id == CurrentMech.Id);

                if (existingMech == null) return;

                int changesCount = 0;

                foreach (var mapObj in CurrentMech.MapObjects)
                {
                    if (!IsValidData(mapObj)) break; // Или continue, если хотите пропустить только битую строку

                    // Ищем оригинал по Id для сравнения или обновления
                    MapObject? originalObj = existingMech.MapObjects?
                        .FirstOrDefault(x => x.Id == mapObj.Id);

                    if (mapObj.ToDelete)
                    {
                        // Удаление
                        if (originalObj != null)
                        {
                            // Вариант А: Жесткое удаление из БД
                            //dbContext.SensorAssignments.Remove(originalSa);

                            // Вариант Б: Мягкое удаление (если есть поле IsDeleted), тогда State = Modified + флаг
                            //originalSa.ToDelete = true;
                            dbContext.Entry(originalObj).State = EntityState.Deleted;

                            changesCount++;
                        }
                    }
                    else if (mapObj.IsNew || mapObj.Id == 0)
                    {
                        // Добавление новой записи
                        // Важно: убедитесь, что у нового объекта сброшена навигация обратно к родителю,
                        // иначе возникнет ошибка ссылающегося ключа (Foreign Key constraint).

                        var temp = mapObj;
                        temp.Id = 0;
                        dbContext.Entry(temp).State = EntityState.Added;
                        changesCount++;
                    }
                    else if (mapObj.IsModified)
                    {
                        // Обновление существующей записи
                        if (originalObj != null)
                        {
                            // Оптимальный способ: обновит только измененные поля
                            dbContext.Entry(originalObj).CurrentValues.SetValues(mapObj);
                            changesCount++;
                        }
                        else
                        {
                            // Запись должна быть в базе, но ее нет в загруженном графе (например, сработал QueryFilter)
                            // Присоединяем как модифицированную
                            dbContext.Attach(mapObj).State = EntityState.Modified;
                            changesCount++;
                        }
                    }

                    // Сброс локальных флагов UI-модели, чтобы следующий клик "Сохранить" не делал лишнего
                    mapObj.IsNew = false;
                    mapObj.IsModified = false;
                    mapObj.ToDelete = false;
                }

                if (changesCount > 0)
                {
                    try
                    {
                        int affectedRows = await dbContext.SaveChangesAsync();
                        HasChanges = false;
                        Growl.Success("Данные сохранены");
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        Growl.Error("Данные были изменены другим пользователем. Попробуйте снова.");
                    }
                }
                else
                {
                    Growl.Error("Нет изменений для сохранения");
                }
            }
        }

        private bool IsValidData(MapObject sensor)
        {
            if (mapObj.Width <= 10 || mapObj.Height <= 10) return false;
            if (mapObj.X <= 0 || mapObj.Y <= 0) return false;
            return true;
        }

        public ICommand SaveSensorPlace { get; }
        public ICommand ShowSensorMechanism { get; }
        public ICommand ShowScheme { get; }
        public ICommand NavigateToSectors { get; }
        public ICommand AddSensorToMap { get; }
        public ICommand DeleteSensorCommand { get; }
        public ICommand TransformSensorCommand { get; }
        public ICommand DragSensorCommand { get; }
        public ICommand DragDeviceCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
    }
}
