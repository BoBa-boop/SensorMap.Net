using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class Devices_VM : ReactiveObject, IActivatableViewModel
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly INavigation _nav;
        private readonly IJsonSerialization _json;
        private readonly IDataService _service;
        private readonly ITempImage _imgManag;
        private readonly IFileManagment _fileManagment;
        private Dictionary<string, AdditionalData> _jsonMoreDataCache = new();
        private List<AdditionalData> addDataList;
        private TreeViewCollection<DeviceType, Device> deviceTree;
        private ObservableCollection<Device> devices;
        private IAppDbContextFactory _appDbContextFactory;
        private Device _selectedDevice;
        private ObservableCollection<Mechanism> _FilteredMechanisms = new ObservableCollection<Mechanism>();
        private bool isEditMode;
        [Reactive] public Device SelectedDevice
        {
            get => _selectedDevice;
            set { if (value != null) this.RaiseAndSetIfChanged(ref _selectedDevice, value); }
        }
        [Reactive] public ObservableCollection<Device> Devices {
            get => devices;
            set { this.RaiseAndSetIfChanged(ref devices, value); }
        }
        private ObservableCollection<DeviceType> _deviceTypes { get; set; }
        [Reactive] public TreeViewCollection<DeviceType, Device> DeviceTree {
            get => deviceTree;
            set { this.RaiseAndSetIfChanged(ref deviceTree, value); }
        }
        [Reactive] public ObservableCollection<Mechanism> FilteredMechanisms
        {
            get => _FilteredMechanisms;
            set { this.RaiseAndSetIfChanged(ref _FilteredMechanisms, value); }
        }
        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }
        

        public Devices_VM(INavigation nav, IJsonSerialization json,
            IDataService service, IAppDbContextFactory appDbContextFactory,
            IFileManagment fileManagment, ITempImage imgMang, Device device = null)
        {
            _service = service;
            _nav = nav;
            _imgManag = imgMang;
            _fileManagment = fileManagment;
            _json = json;
            _appDbContextFactory = appDbContextFactory;
            
            SelectedDevice = device;
            
            NavigateToMech = new RelayCommand<Mechanism>((mech) =>
            {
                if (mech == null) return;
                _nav.NavigateTo<MechanismVM>(mech);
            });

            OpenFullScreen = new RelayCommand<byte[]>((image) =>
            {
                _imgManag.OpenFullScreen(_imgManag.CreateImageFromBytes(image!));
            }, (image) => { return image != null; });

            ShowAllFiles = new RelayCommand(() =>
            {
                foreach (var item in SelectedDevice.Files)
                {
                    item.IsHide = false;
                }
            });

            SaveMoreData = new RelayCommand(SaveDataFileds);
            
            AddFiles = new RelayCommand<Device>((d) => 
            {
                string[]paths = _fileManagment.OpenFileDialog(true);
                _fileManagment.AddHelpfulFile(paths,d); 
            });
            DeletePathFiles = new RelayCommand<HelpfulFile>((file) =>
            {
                try
                {
                    using (var _dbContext = _appDbContextFactory.CreateDbContext())
                    {
                        var fileInDb = _dbContext.HelpfulFiles.Find(file!.Id);

                        if (fileInDb != null)
                        {
                            _dbContext.HelpfulFiles.Remove(fileInDb);
                            _dbContext.SaveChanges();
                        }
                        SelectedDevice.Files.Remove(file);
                    }
                    Growl.Success(new GrowlInfo
                    {
                        Message = "Путь к файлам удален.",
                        CancelStr = "Ignore",
                        ShowDateTime = false,
                        WaitTime = 2
                    });
                }
                catch (Exception ex)
                {
                    Growl.Error("Ошибка при удаление путей!");
                    Logger.Error(ex.Message);
                }
            }, (f) => { return f != null; });
            OpenFile = new RelayCommand<HelpfulFile>((file) => 
            {
                if (!_fileManagment.OpenFileInExplorer(file.NameFile))
                {
                    MessageBoxResult res = HandyControl.Controls.MessageBox.Show(
                        new MessageBoxInfo
                        {
                            Message = "Файл не найден, скрыть файл?",
                            Caption = "Ошибка",
                            DefaultResult = MessageBoxResult.No
                        });
                    if (res == MessageBoxResult.OK)
                    {
                        file.IsHide = true;
                    }
                }
            });
            SaveFiles = new RelayCommand(() =>
            {
                try
                {
                    
                    using (var _dbContext = _appDbContextFactory.CreateDbContext())
                    {
                        _dbContext.Attach(SelectedDevice);
                        _dbContext.Entry(SelectedDevice).Collection(s => s.Files).IsModified = true;
                        bool success = _dbContext.SaveChanges() > 0 ? true : false;
                        if (success) UnSetIsNew(SelectedDevice.Files);

                    }
                    Growl.Success(new GrowlInfo
                    {
                        Message = "Путь к файлам сохранен.",
                        CancelStr = "Ignore",
                        ShowDateTime = false,
                        WaitTime = 2
                    });
                    
                }
                catch (Exception ex)
                {
                    Growl.Error("Ошибка при сохранение путей!");
                    Logger.Error(ex.Message);
                }

            });
            this.WhenActivated(async disposables =>
            {
                _service.WhenAnyValue(x => x.IsEditMode)
                    .BindTo(this, x => x.IsEditMode)
                    .DisposeWith(disposables);

                    _service.WhenAnyValue(x => x.IsEditMode)
                        .BindTo(this, x => x.IsEditMode)
                        .DisposeWith(disposables);

                    //Этап заполнения словаря с доп информацией
                    if (File.Exists("DevicesMoreData.json"))
                    {
                        var list = _json.ReadFromJsonFile<List<AdditionalData>>("DevicesMoreData.json");
                        _jsonMoreDataCache = list?.Where(x => x.Name != null).ToDictionary(x => x.Name, x => x) ?? new();
                    }
                    //Этап заполнения древовидной структуры названиями
                    using (var _dbContext = _appDbContextFactory.CreateDbContext())
                    {
                        var queryTypes = await _dbContext.DeviceTypes
                                .AsNoTracking()
                                .Select(x => new DeviceType()
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    Characteristics = x.Characteristics
                                })
                                .ToListAsync();

                        var queryDevices = await _dbContext.Devices
                            .AsNoTracking()
                            .Select(x => new Device()
                            {
                                Id = x.Id,
                                Name = x.Name,
                                DeviceTypeId = x.DeviceTypeId
                            })
                            .ToListAsync();

                        _deviceTypes = new(queryTypes);
                        Devices = new(queryDevices);

                        Func<DeviceType, Device, bool> filter = (type, device) => device.DeviceTypeId == type.Id;
                        DeviceTree = new TreeViewCollection<DeviceType, Device>("Name", new(_deviceTypes), Devices, filter);

                    }

                    //Этап фильтрации механизмов
                    this.WhenAnyValue(x => x.SelectedDevice)
                        .Where(device => device != null)
                        .SelectMany(async device =>
                        {
                            // Создаем временный контекст ИМЕННО на время выполнения этого запроса
                            using var dbContext = appDbContextFactory.CreateDbContext();

                            // Все запросы делаем через локальный dbContext
                            return await dbContext.Mechanisms
                                .AsNoTracking()
                                .Where(mech => mech.MapObjects
                                    .OfType<DeviceAssignment>()
                                    .Any(da => da.DeviceId == device.Id))
                                .Select(x => new Mechanism
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    SectorID = x.SectorID,
                                    MapObjects = new(x.MapObjects
                                    .Select(k => new SensorAssignments() { MechanismId = k.MechanismId }).ToList())
                                })
                                .ToListAsync(); // Асинхронное выполнение
                        })
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(filteredList =>
                        {
                            FilteredMechanisms = new(filteredList);
                        })
                        .DisposeWith(disposables);


                    this.WhenAnyValue(x => x.SelectedDevice)
                        .Where(device => device != null)
                        .SelectMany(async selectedDevice =>
                        {
                            // 1. Сначала подготавливаем AdditionalData (синхронно в фоновом таске)
                            if (selectedDevice.AdditionalData == null)
                            {
                                if (_jsonMoreDataCache.TryGetValue(selectedDevice.Name, out var cachedData) && cachedData.HasData())
                                {
                                    selectedDevice.AdditionalData = cachedData;
                                }
                                else
                                {
                                    var sType = _deviceTypes.FirstOrDefault(t => t.Id == selectedDevice.DeviceTypeId);
                                    if (sType != null && sType.Characteristics?.Any() == true)
                                    {
                                        selectedDevice.AdditionalData = AdditionalData.CreateRecord(selectedDevice.Name, sType.Characteristics);
                                        selectedDevice.AdditionalData.Data = new(sType.Characteristics.Select(c => new MoreData
                                        {
                                            Parameter = c.Title,
                                            Value = string.Empty
                                        }));
                                    }
                                }
                            }

                            // 2. Проверяем, нужно ли лезть в БД за тяжелыми данными
                            if (selectedDevice.Image != null || selectedDevice.Files?.Any() == true)
                            {
                                return selectedDevice; // Данные уже есть, возвращаем датчик
                            }

                            // 3. Догружаем тяжелые данные из БД
                            using var dbContext = _appDbContextFactory.CreateDbContext();
                            var heavyData = await dbContext.Devices
                                .AsNoTracking()
                                .Where(s => s.Id == selectedDevice.Id)
                                .Select(s => new { s.Image, s.Files, s.DeviceType })
                                .FirstOrDefaultAsync();

                            if (heavyData != null)
                            {
                                selectedDevice.Image = heavyData.Image;
                                selectedDevice.Files = heavyData.Files;
                                selectedDevice.DeviceType = heavyData.DeviceType;
                            }
                            return selectedDevice;
                        })
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(sensor =>
                        {
                            this.RaisePropertyChanged(nameof(SelectedDevice));
                        })
                        .DisposeWith(disposables);
                });
        }

        
        private void SaveDataFileds()
        {
            string FILE_PATH = "DevicesMoreData.json";
            try
            {
                if (SelectedDevice.AdditionalData == null) return;

                SelectedDevice.AdditionalData.Name = SelectedDevice.Name;
                var editableObject = addDataList.Where(x => x.Name == SelectedDevice.Name).FirstOrDefault();
                if (editableObject != null)
                {
                    editableObject.Data = SelectedDevice.AdditionalData.Data;
                }
                else
                {
                    addDataList.Add(SelectedDevice.AdditionalData);
                }
                _json.WriteToJsonFile<List<AdditionalData>>(FILE_PATH, addDataList);
                Growl.Success(new GrowlInfo
                {
                    Message = "Дополнительные данные сохранены!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
            catch
            {
                Growl.Error(new GrowlInfo
                {
                    Message = "Не удалось сохранить данные!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
        }
        public ICommand NavigateToMech { get; }
        public ICommand SaveMoreData { get; }
        public ICommand SaveFiles { get; }
        public ICommand OpenFile { get; }
        public ICommand DeletePathFiles { get; }
        public ICommand AddFiles {get;}
        public ICommand OpenFullScreen { get; }
        public ICommand ShowAllFiles { get; }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        private void UnSetIsNew(IEnumerable<HelpfulFile> files)
        {
            foreach (var item in files)
            {
                item.IsNew = false;
            }
        }
    }
}
