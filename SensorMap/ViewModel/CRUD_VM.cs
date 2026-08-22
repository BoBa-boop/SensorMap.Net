using CommunityToolkit.Mvvm.Input;
using DynamicData;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Properties.Langs;
using HandyControl.Tools;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Behaviors;
using SensorMap.Converters;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SensorMap.ViewModel
{
    public class CRUD_VM:ReactiveObject
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IDataBaseProvider _provider;
        private readonly IFileManagment fileManagment;
        private readonly IJsonSerialization _json;
        private readonly IDataService _service; 
        private readonly ITempImage _tempImage;
        private IAppDbContextFactory _appDbContextFactory;
        private bool isEditMode;
        private ICollectionView mechanisms;
        private ICollectionView devices;
        private ICollectionView sensors;
        private readonly AppDBContext _dbContext;
        private bool _loadInProgress;
        private int _pendingTabIndex = -1;
        private bool _sectorsLoaded;
        private bool _mechanismsLoaded;
        private bool _sensorsLoaded;
        private bool _devicesLoaded;
        private bool _typesLoaded;
        private int _loadingTabs;
        private int selectedTabIndex;
        private bool isLoading;
        public readonly UndoRedoStack _undoRedoManager = new UndoRedoStack();
        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }
        [Reactive] public bool CanUndo => _undoRedoManager.CanUndo;
        [Reactive] public bool CanRedo=>_undoRedoManager.CanRedo;
        [Reactive] public INavigation Navigation { get; set; }
        [Reactive] public ObservableCollection<Sector> Sectors { get; set; }
        [Reactive] public ICollectionView Sensors { get => sensors; set => this.RaiseAndSetIfChanged(ref sensors, value); }
        [Reactive] public ICollectionView Devices { get => devices; set => this.RaiseAndSetIfChanged(ref devices, value); }
        [Reactive] public ObservableCollection<SensorType> SensorTypes { get; set; }
        [Reactive] public ObservableCollection<DeviceType> DeviceTypes { get; set; }
        [Reactive] public ObservableCollection<string> Manufacturers { get; set; }
        [Reactive] public ICollectionView Mechanisms { get => mechanisms; set => this.RaiseAndSetIfChanged(ref mechanisms, value); }
        [Reactive] public int SelectedTabIndex { get => selectedTabIndex; set => this.RaiseAndSetIfChanged(ref selectedTabIndex, value); }
        [Reactive] public bool IsLoading { get => isLoading; set => this.RaiseAndSetIfChanged(ref isLoading,value); }

        public CRUD_VM(IDataBaseProvider provider, IDataService service, IAppDbContextFactory cxFactory,
            IJsonSerialization json, INavigation nav, ITempImage tempImage, IFileManagment _fileManagment)
        {
            Navigation = nav;
            _tempImage = tempImage;
            _json = json;
            _provider = provider;
            fileManagment = _fileManagment;
            _appDbContextFactory = cxFactory;
            _service = service;
            _dbContext = _appDbContextFactory.CreateDbContext();
            #region Commands
            ShowCommand = new RelayCommand<object>((obj) =>
            {
                if (obj is Sensor)
                    Navigation.NavigateTo<SensorVM>(obj);
                if (obj is Device)
                    Navigation.NavigateTo<Devices_VM>(obj);
            });
            SaveCommand = new RelayCommand<object>((arg) =>
            {
                if (arg is null) return;
                var name = arg.GetType()?.GetProperty("Name")?.GetValue(arg);
                using (var dBContext = _appDbContextFactory.CreateDbContext())
                {
                    try
                    {
                        var original = _service.GetOriginalEntry(dBContext, arg);
                        if (original != null && dBContext.ChangeTracker.HasChanges())
                            dBContext.Entry(original).CurrentValues.SetValues(arg);
                        else
                        {
                            if(original!=null)
                                dBContext.Entry(original).State = EntityState.Detached;
                            dBContext.Update(arg);
                        }
                        foreach (var fileEntry in dBContext.ChangeTracker.Entries<HelpfulFile>())
                        {
                            if (fileEntry.State != EntityState.Added)
                                fileEntry.Property(f => f.ImageFile).IsModified = false;
                        }
                        dBContext.SaveChanges();
                        arg.GetType()?.GetProperty("IsModified")?.SetValue(arg, false);
                        Growl.Success(new GrowlInfo
                        {
                            Message = "Данные в БД изменены",
                            CancelStr = "Ignore",
                            ShowDateTime = false,
                            WaitTime = 2
                        });
                    }
                    catch
                    {
                        Growl.Error("Ошибка при изменение БД");
                        Logger.Error("Ошибка при изменение {0} в БД,", name);
                    }
                }

            });
            DeleteCommand = new RelayCommand<object>((arg) =>
            {
                if (arg is null) return;
                object[] values = (object[])arg;
                var entityType = values[0].GetType();
                var collection = values[1];
                
                try
                {
                    using (var dBContext = _appDbContextFactory.CreateDbContext())
                    {
                        var result = System.Windows.MessageBox.Show("Вы действительно хотите удалить строчку?", "Подтверждение",
                                MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            if (dBContext.Entry(values[0]).IsKeySet)
                            {
                                dBContext.Remove(values[0]);
                                dBContext.SaveChanges();
                                Growl.Success(new GrowlInfo
                                {
                                    Message = "Данные удалены!",
                                    CancelStr = "Ignore",
                                    ShowDateTime = false,
                                    WaitTime = 2
                                });
                            }
                            if (values[1] is ICollectionView collectionView)
                            {
                                collection = collectionView.SourceCollection;
                                if (collection is IList list)
                                    list?.Remove(values[0]);
                                collectionView.Refresh();
                            }
                            else if(collection is IList list)
                                list?.Remove(values[0]);


                        }
                    }
                }
                catch
                {
                    Growl.Error(new GrowlInfo
                    {
                        Message = "Ошибка при удаление!",
                        CancelStr = "Ignore",
                        ShowDateTime = false,
                        WaitTime = 2
                    });
                }


            });

            AddImage = new RelayCommand<object>((arg) =>
            {
                if (arg is null) return;
                var entityType = arg.GetType();
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Файлы рисунков (*.bmp, *.jpg, *.png, *.jpeg)|*.bmp;*.jpg;*.png;*.jpeg|Все файлы (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    // Прочитаем выбранный файл в массив байтов
                    using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        byte[] photoBytes = new byte[fs.Length];
                        fs.ReadExactly(photoBytes);
                        entityType.GetProperty("Image")?.SetValue(arg, photoBytes);
                        entityType.GetProperty("IsModified")?.SetValue(arg, true);
                    }
                }
            });
            ShowPreviewImage = new RelayCommand<object>((image) =>
            {
                if (image is byte[] img && img != null)
                {
                    var browser = new CustomImageBrowser(_tempImage.CreateImageFromBytes(img)) { Title = "Просмотр схемы" };
                    browser.ShowDialog();
                }
            });
            AddSensorType = new RelayCommand<object>((param) =>
            {
                if (param is null) return;

                var values = (object[])param;
                var name = (string)values[0];
                var color = (string)values[1].ToString();
                SensorType sType = new SensorType();
                sType.Name = name;
                sType.Color = color;

                if (!SensorTypes.Where(x => x.Name == sType.Name).Any())
                {
                    sType.IsNew = true;
                    SensorTypes.Add(sType);
                }
            }, (param) =>
            {
                if (param == null) return false;
                var values = (object[])param;
                return !string.IsNullOrWhiteSpace(values[0].ToString());
            });
            AddDeviceType = new RelayCommand<object>((param) =>
            {
                if (param is null) return;

                var name = (string)param;
                DeviceType sType = new DeviceType();
                sType.Name = name;
                if (!DeviceTypes.Where(x => x.Name == sType.Name).Any())
                {
                    sType.IsNew = true;
                    DeviceTypes.Add(sType);
                }
            }, (param) =>
            {
                if (param == null) return false;
                var value = (string)param;
                return !string.IsNullOrWhiteSpace(value.ToString());
            });
            DeleteNodeTitleType = new RelayCommand<object>((type) =>
            {
                if (type is SensorType sensorType && sensorType != null)
                {
                    var res = System.Windows.MessageBox.Show("Удаление типа, также включает УДАЛЕНИЕ датчиков и параметров этого типа! Подтвердите действие.",
                        "Подтверждение действий", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.OK)
                    {
                        SensorTypes.Remove(sensorType);
                        using (var dBContext = _appDbContextFactory.CreateDbContext())
                        {
                            if (dBContext.SensorTypes.Contains(sensorType))
                            {
                                dBContext.SensorTypes.Remove(sensorType);
                                dBContext.SaveChanges();
                            }
                        }
                    }
                }
                if (type is DeviceType deviceType && deviceType != null)
                {
                    var res = System.Windows.MessageBox.Show("Удаление типа, также включает УДАЛЕНИЕ устройств и параметров этого типа! Подтвердите действие.",
                        "Подтверждение действий", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.OK)
                    {
                        DeviceTypes.Remove(deviceType);
                        using (var dBContext = _appDbContextFactory.CreateDbContext())
                        {
                            if (dBContext.DeviceTypes.Contains(deviceType))
                            {
                                dBContext.DeviceTypes.Remove(deviceType);
                                dBContext.SaveChanges();
                            }
                        }
                    }
                }
            },
            (type) => { return type != null; });

            AddCharacteristic = new RelayCommand<object>((type) =>
            {
                if (type is SensorType sensorType)
                    sensorType.Characteristics.Add(new SensorCharacteristic() { Title = "Новая характеристика", SensorTypeId = sensorType.Id });
                if (type is DeviceType deviceType)
                    deviceType.Characteristics.Add(new DeviceCharacteristic() { Title = "Новая характеристика", DeviceTypeId = deviceType.Id });
            }, (type) => { return type != null; });
            DeleteCharacteristic = new RelayCommand<object>((type) =>
            {
                if (type is SensorCharacteristic sensorCharact && sensorCharact != null)
                {
                    var res = System.Windows.MessageBox.Show("Операция включает в себя удаление записанных данных в выбранный параметр.\r" +
                        $"Датчики имеющий тип {sensorCharact.Title} также утратят параметр и данные! Подтвердите действие.",
                        "Подтверждение действий", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.OK)
                    {
                        using (var dBContext = _appDbContextFactory.CreateDbContext())
                        {

                            dBContext.SensorCharacteristic.Remove(sensorCharact);
                            dBContext.SaveChanges();
                        }
                        DeleteFromFile(sensorCharact);
                    }

                }
                if (type is DeviceCharacteristic deviceCharacteristic && deviceCharacteristic != null)
                {
                    var res = System.Windows.MessageBox.Show("Операция включает в себя удаление записанных данных в выбранный параметр.\r" +
                        $"Датчики имеющий тип {deviceCharacteristic.Title} также утратят параметр и данные! Подтвердите действие.",
                        "Подтверждение действий", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.OK)
                    {
                        using (var dBContext = _appDbContextFactory.CreateDbContext())
                        {
                            if (deviceCharacteristic.Id != 0)
                            {
                                dBContext.DeviceCharacteristic.Remove(deviceCharacteristic);
                                dBContext.SaveChanges();
                            }

                        }
                        DeleteFromFile(deviceCharacteristic);
                    }

                }
            }, (type) => { return type != null; });

            ShowColorPicker = new RelayCommand<object>((s) =>
            {
                if (s is SensorType sensorType)
                {
                    var picker = new HandyControl.Controls.ColorPicker();
                    var window = new PopupWindow
                    {
                        PopupElement = picker,
                        ShowBorder = false,
                        ShowTitle = false,
                        AllowsTransparency = true,
                        WindowStyle = WindowStyle.None

                    };
                    picker.SelectedColorChanged += delegate { sensorType.Color = picker.SelectedBrush.ToString(); };
                    picker.Confirmed += delegate { sensorType.Color = picker.SelectedBrush.ToString(); window.Close(); };
                    picker.Canceled += delegate { window.Close(); };
                    window.Show();
                }
            }, (s) => { return s != null; });
            AddHelpfulFile = new RelayCommand<object>((obj) =>
            {
                if (obj is Mechanism mech)
                {
                    string[] paths = fileManagment.OpenFileDialog(true);
                    mech.IsModified = fileManagment.AddHelpfulFile(paths, obj);
                }
            });
            ShowHelpfulFile = new RelayCommand<object>((obj) => 
            {
                if(obj is Mechanism mech)
                {
                    if (mech == null || mech.Files == null || !mech.Files.Any()) return;
                    fileManagment.OpenFileInExplorer(mech.Files.First().NameFile);
                }
            });
            UndoCommand = new RelayCommand(_undoRedoManager!.Undo);
            RedoCommand = new RelayCommand(_undoRedoManager.Redo);
            #endregion
            _service.WhenAnyValue(x => x.IsEditMode)
                .BindTo(this, x => x.IsEditMode);

            _undoRedoManager.WhenAnyValue(x => x.CanUndo)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(CanUndo)));

            _undoRedoManager.WhenAnyValue(x => x.CanRedo)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(CanRedo)));

            this.WhenAnyValue(x => x.SelectedTabIndex)
                .Subscribe(RequestLoad);

            //RequestLoad(0);
        }

        private void RequestLoad(int index)
        {
            _pendingTabIndex = index;
            if (_loadInProgress) return;
            _ = RunLoadLoopAsync();
        }

        private async Task RunLoadLoopAsync()
        {
            _loadInProgress = true;
            try
            {
                while (_pendingTabIndex >= 0)
                {
                    var index = _pendingTabIndex;
                    _pendingTabIndex = -1;
                    await LoadTabAsync(index);
                }
            }
            finally
            {
                _loadInProgress = false;
            }
        }

        private void SetLoading(bool add)
        {
            _loadingTabs = Math.Max(0, _loadingTabs + (add ? 1 : -1));
            IsLoading = _loadingTabs > 0;
        }

        private async Task LoadTabAsync(int index)
        {
            SetLoading(true);
            try
            {
                switch (index)
                {
                    case 0:
                        await LoadSectorsAsync();
                        await LoadMechanismsAsync();
                        break;
                    case 1:
                        await LoadSectorsAsync();
                        await LoadDevicesAsync();
                        await LoadMechanismsAsync();
                        break;
                    case 2:
                        await LoadTypesAsync();
                        await LoadSensorsAsync();
                        break;
                    case 3:
                        await LoadTypesAsync();
                        await LoadDevicesAsync();
                        await LoadMechanismsAsync();
                        break;
                    case 4:
                        await LoadTypesAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка загрузки данных вкладки {0}", index);
                Growl.Error("Ошибка при загрузке данных из БД");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async Task LoadTypesAsync()
        {
            if (_typesLoaded) return;
            var sensorTypes = await _dbContext.SensorTypes.Include(x => x.Characteristics).ToListAsync();
            var deviceTypes = await _dbContext.DeviceTypes.Include(x => x.Characteristics).ToListAsync();
            SensorTypes = new ObservableCollection<SensorType>(sensorTypes);
            DeviceTypes = new ObservableCollection<DeviceType>(deviceTypes);
            _typesLoaded = true;
        }

        private async Task LoadSectorsAsync()
        {
            if (_sectorsLoaded) return;
            var sectors = await _dbContext.Sectors.ToListAsync();
            Sectors = new ObservableCollection<Sector>(sectors);
            _sectorsLoaded = true;
        }

        private async Task LoadDevicesAsync()
        {
            if (_devicesLoaded) return;
            var devices = await _dbContext.Devices.ToListAsync();
            Devices = CollectionViewSource.GetDefaultView(devices);
            ConfigureDevicesView();
            _devicesLoaded = true;
        }

        private async Task LoadSensorsAsync()
        {
            if (_sensorsLoaded) return;
            var sensors = await _dbContext.Sensors.ToListAsync();
            Sensors = CollectionViewSource.GetDefaultView(sensors);
            ConfigureSensorsView();
            _sensorsLoaded = true;
        }

        private async Task LoadMechanismsAsync()
        {
            if (_mechanismsLoaded) return;
            var mechanisms = await _dbContext.Mechanisms.ToListAsync();
            var files = await _dbContext.HelpfulFiles.AsNoTracking()
                .Where(f => f.MechanismId != null)
                .Select(f => new HelpfulFile { Id = f.Id, NameFile = f.NameFile, MechanismId = f.MechanismId })
                .ToListAsync();
            foreach (var m in mechanisms)
            {
                m.Files = new ObservableCollection<HelpfulFile>(files.Where(f => f.MechanismId == m.Id));
            }
            Mechanisms = CollectionViewSource.GetDefaultView(mechanisms);
            ConfigureMechanismsView();
            _mechanismsLoaded = true;
        }

        private void ConfigureMechanismsView()
        {
            using (Mechanisms.DeferRefresh())
            {
                Mechanisms.SortDescriptions.Add(new SortDescription("Sector.Name", ListSortDirection.Ascending));
                Mechanisms.GroupDescriptions.Add(new PropertyGroupDescription("Sector.Name"));
                var groupDescription = new PropertyGroupDescription("Name", new EqualMechGroup());
                Mechanisms.GroupDescriptions.Add(groupDescription);
            }
        }

        private void ConfigureDevicesView()
        {
            using (Devices.DeferRefresh())
            {
                Devices.SortDescriptions.Add(new SortDescription("DeviceType.Name", ListSortDirection.Ascending));
                Devices.GroupDescriptions.Add(new PropertyGroupDescription("DeviceType.Name"));
            }
        }

        private void ConfigureSensorsView()
        {
            using (Sensors.DeferRefresh())
            {
                Sensors.SortDescriptions.Add(new SortDescription("SensorType.Name", ListSortDirection.Ascending));
                Sensors.GroupDescriptions.Add(new PropertyGroupDescription("SensorType.Name"));
            }
        }

        private void DeleteFromFile(object characteristic)
        {
            if(characteristic is SensorCharacteristic sensorCharacteristic)
            {
                if (File.Exists("SensorMoreData.json"))
                {
                    var file = _json.ReadFromJsonFile<List<AdditionalData>>("SensorMoreData.json");
                    foreach (var item in file)
                    {
                        item.Data.RemoveMany(item.Data.Where(x => x.Parameter == sensorCharacteristic.Title));
                    }
                    _json.WriteToJsonFile("SensorMoreData.json", file);
                }
            }
            if (characteristic is DeviceCharacteristic deviceCharacteristic)
            {
                if (File.Exists("DeviceMoreData.json"))
                {
                    var file = _json.ReadFromJsonFile<List<AdditionalData>>("DeviceMoreData.json");
                    foreach (var item in file)
                    {
                        item.Data.RemoveMany(item.Data.Where(x => x.Parameter == deviceCharacteristic.Title));
                    }
                    _json.WriteToJsonFile("DeviceMoreData.json", file);
                }
            }
        }

        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand ShowCommand { get; set; }
        public ICommand AddImage {  get; set; }
        public ICommand ShowPreviewImage { get; set; }
        public ICommand ShowColorPicker { get; }
        public ICommand AddSensorType { get; }
        public ICommand AddDeviceType { get; }
        public ICommand DeleteNodeTitleType { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand AddHelpfulFile { get; }
        public ICommand ShowHelpfulFile { get; }
        public ICommand AddCharacteristic { get; set; }
        public ICommand DeleteCharacteristic { get; set; }
        public void RecordEdit<T>(T _inputObject, string propertyName, object oldValue, object newValue)
        {
            var command = new Commands.DataGridCommands.EditCell<T>(_inputObject, propertyName, oldValue, newValue);
            _undoRedoManager.Do(command);
        }
        
    }
}
