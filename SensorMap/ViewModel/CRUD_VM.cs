using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
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
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reactive.Disposables;
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
    public class CRUD_VM : ReactiveObject, IActivatableViewModel
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
        private bool _loadInProgress;
        private int _pendingTabIndex = -1;
        private bool _firstPageLoaded;
        private bool _secondPageLoaded;
        private bool _thirdPageLoaded;
        private bool _fourthLoaded;
        private int selectedTabIndex;
        private bool isLoading;
        private ObservableCollection<SensorType> sensorTypes;
        private ObservableCollection<DeviceType> deviceTypes;
        private ObservableCollection<Sector> sectors;
        private SerialDisposable serialDisposable = new();
        public readonly UndoRedoStack _undoRedoManager = new UndoRedoStack();
        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }
        [Reactive] public bool CanUndo => _undoRedoManager.CanUndo;
        [Reactive] public bool CanRedo=>_undoRedoManager.CanRedo;
        [Reactive] public INavigation Navigation { get; set; }
        [Reactive] public ObservableCollection<Sector> Sectors { get => sectors; set => this.RaiseAndSetIfChanged(ref sectors, value); }
        [Reactive] public ICollectionView Sensors { get => sensors; set => this.RaiseAndSetIfChanged(ref sensors, value); }
        [Reactive] public ICollectionView Devices { get => devices; set => this.RaiseAndSetIfChanged(ref devices, value); }
        [Reactive] public ObservableCollection<SensorType> SensorTypes 
        { 
            get => sensorTypes; 
            set => this.RaiseAndSetIfChanged(ref sensorTypes,value); 
        }
        [Reactive] public ObservableCollection<DeviceType> DeviceTypes { get => deviceTypes; set => this.RaiseAndSetIfChanged(ref deviceTypes,value); }
        [Reactive] public ICollectionView Mechanisms { get => mechanisms; set => this.RaiseAndSetIfChanged(ref mechanisms, value); }
        [Reactive] public int SelectedTabIndex { get => selectedTabIndex; set => this.RaiseAndSetIfChanged(ref selectedTabIndex, value); }
        [Reactive] public bool IsLoading { get => isLoading; set => this.RaiseAndSetIfChanged(ref isLoading,value); }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

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
                            if (original != null)
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
                            else if (collection is IList list)
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
                {
                    sensorType.Characteristics.Add(new SensorCharacteristic() { Title = "Новая характеристика", SensorTypeId = sensorType.Id });
                    sensorType.IsNew = true;
                }
                if (type is DeviceType deviceType)
                    deviceType.Characteristics.Add(new DeviceCharacteristic() { Title = "Новая характеристика", DeviceTypeId = deviceType.Id });
            }, (type) => { return type != null; });
            DeleteCharacteristic = new RelayCommand<object[]>((obj) =>
            {
                if (obj[0] is SensorType sensorType && sensorType != null)
                {
                    SensorCharacteristic characteristic = obj[1] as SensorCharacteristic ?? new SensorCharacteristic();
                    using (var dBContext = _appDbContextFactory.CreateDbContext())
                    {
                        if (characteristic.Id != 0)
                        {
                            var res = System.Windows.MessageBox.Show("Операция включает в себя удаление записанных данных в выбранный параметр.\r" +
                            $"Датчики имеющий тип {sensorType.Name} также утратят параметр и данные! Подтвердите действие.",
                            "Подтверждение действий", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                            if (res == MessageBoxResult.OK)
                            {

                                dBContext.SensorCharacteristic.Remove(characteristic);
                                dBContext.SaveChanges();

                                DeleteFromFile(characteristic);
                            }
                        }
                        else
                        {
                            sensorType.Characteristics?.Remove(characteristic);
                        }
                    }

                }
                if (obj[0] is DeviceType deviceType && deviceType != null)
                {
                    DeviceCharacteristic characteristic = obj[1] as DeviceCharacteristic ?? new DeviceCharacteristic();
                    using (var dBContext = _appDbContextFactory.CreateDbContext())
                    {
                        var res = System.Windows.MessageBox.Show("Операция включает в себя удаление записанных данных в выбранный параметр.\r" +
                        $"Датчики имеющий тип {deviceType.Name} также утратят параметр и данные! Подтвердите действие.",
                        "Подтверждение действий", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        if (res == MessageBoxResult.OK)
                        {

                            if (deviceType.Id != 0)
                            {
                                dBContext.DeviceCharacteristic.Remove(characteristic);
                                dBContext.SaveChanges();
                            }
                            DeleteFromFile(characteristic);
                        }
                        else
                        {
                            deviceType.Characteristics?.Remove(characteristic);
                        }

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
                    picker.SelectedColorChanged += delegate { sensorType.Color = picker.SelectedBrush.ToString(); sensorType.IsNew = true; };
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
                if (obj is Mechanism mech)
                {
                    if (mech == null || mech.Files == null || !mech.Files.Any()) return;
                    fileManagment.OpenFileInExplorer(mech.Files.First().NameFile);
                }
            });
            UndoCommand = new RelayCommand(_undoRedoManager!.Undo);
            RedoCommand = new RelayCommand(_undoRedoManager.Redo);
            RecordEditCommand = new RelayCommand<IUndoRedoCommand>(command =>
            {
                if (command != null)
                {
                    _undoRedoManager.Do(command);
                    //Mechanisms.Refresh();
                }
            });
            #endregion



            this.WhenActivated(disposables =>
            {
            _service.WhenAnyValue(x => x.IsEditMode)
                .BindTo(this, x => x.IsEditMode)
                .DisposeWith(disposables);

            _undoRedoManager.WhenAnyValue(x => x.CanUndo)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(CanUndo)))
            .DisposeWith(disposables);

            _undoRedoManager.WhenAnyValue(x => x.CanRedo)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(CanRedo)))
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.SelectedTabIndex)
                .Subscribe(RequestLoad)
                .DisposeWith(disposables);

           
            });
        }
        #region LoadData
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


        private async Task LoadTabAsync(int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        await LoadFirstPageAsync();
                        break;
                    case 1:
                        await LoadSecondAsync();
                        await LoadFourthPageAsync();
                        break;
                    case 2:
                        await LoadThirdAsync();
                        break;
                    case 3:
                        await LoadFourthPageAsync();
                        break;
                    case 4:
                        await LoadThirdAsync();
                        await LoadFourthPageAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка загрузки данных вкладки {0}", index);
                Growl.Error("Ошибка при загрузке данных из БД");
            }
        }

        

        private async Task LoadFirstPageAsync()
        {
            if (_firstPageLoaded) return;
            using var dbContext = _appDbContextFactory.CreateDbContext();
            var sectors = await dbContext.Sectors.AsNoTracking()
                .Include(x=>x.Mechanisms).ToListAsync();
            Sectors = new ObservableCollection<Sector>(sectors);
            _firstPageLoaded = true;
        }
        private async Task LoadSecondAsync()
        {
            if (_secondPageLoaded) return;
            using var dbContext = _appDbContextFactory.CreateDbContext();
            var mechanisms = await dbContext.Mechanisms.AsNoTracking()
                .Include(x=>x.Files)
                .Include(x=>x.Sector)
                .Include(x=>x.Device).ToListAsync();
            
            Mechanisms = CollectionViewSource.GetDefaultView(mechanisms);
            ConfigureMechanismsView();
            _secondPageLoaded = true;
            serialDisposable.Disposable = mechanisms.Select(m => m.WhenAnyValue(x => x.IsModified).Skip(1))
                .Merge()
                .Subscribe(_ => 
                {
                    if (Mechanisms is IEditableCollectionView editableView)
                    {
                        if (editableView.IsEditingItem)
                        {
                            editableView.CommitEdit(); // Завершаем транзакцию редактирования ячейки
                        }
                        if (editableView.IsAddingNew)
                        {
                            editableView.CommitNew();  // Завершаем транзакцию добавления строки
                        }
                    }
                    //Mechanisms.Refresh();
                });
        }
        

        private async Task LoadThirdAsync()
        {
            if (_thirdPageLoaded) return;
            using var dbContext = _appDbContextFactory.CreateDbContext();
            var sensors = await dbContext.Sensors.AsNoTracking()
                .Include(x=>x.SensorType).ToListAsync();
            var sensorTypes = await dbContext.SensorTypes.AsNoTracking()
                .Include(x => x.Characteristics).ToListAsync();
            SensorTypes = new ObservableCollection<SensorType>(sensorTypes);
            Sensors = CollectionViewSource.GetDefaultView(sensors);
            ConfigureSensorsView();
            _thirdPageLoaded = true;
        }
        private async Task LoadFourthPageAsync()
        {
            if (_fourthLoaded) return;
            using var dbContext = _appDbContextFactory.CreateDbContext();
            var devices = await dbContext.Devices
                .Include(x=>x.DeviceType).ToListAsync();
            var deviceTypes = await dbContext.DeviceTypes.AsNoTracking()
                .Include(x => x.Characteristics).ToListAsync();

            DeviceTypes = new ObservableCollection<DeviceType>(deviceTypes);
            Devices = CollectionViewSource.GetDefaultView(devices);

            ConfigureDevicesView();
            _fourthLoaded = true;
        }
        #endregion
        #region GroupView
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
        #endregion
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
        public ICommand RecordEditCommand { get; }
    }
}
