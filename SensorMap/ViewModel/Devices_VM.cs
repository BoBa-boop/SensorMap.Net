using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace SensorMap.ViewModel
{
    public class Devices_VM : ReactiveObject
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IDataBaseProvider _provider;
        private readonly INavigation _nav;
        private readonly IJsonSerialization _json;
        private readonly IDataService _service;
        private readonly ITempImage _imgManag;
        private readonly IFileManagment _fileManagment;
        private IAppDbContextFactory _appDbContextFactory;
        private Device _selectedDevice = new Device();
        private ObservableCollection<Mechanism> _FilteredMechanisms = new ObservableCollection<Mechanism>();
        private bool isEditMode;
        [Reactive] public Device SelectedDevice
        {
            get => _selectedDevice;
            set { if (value != null) this.RaiseAndSetIfChanged(ref _selectedDevice, value); }
        }
        [Reactive] public ObservableCollection<Device> Devices { get; set; }
        private ObservableCollection<DeviceType> _deviceTypes { get; set; }
        [Reactive] public TreeViewCollection<string, Device> DeviceTree { get; set; }
        [Reactive] public ObservableCollection<Mechanism> FilteredMechanisms
        {
            get => _FilteredMechanisms;
            set { this.RaiseAndSetIfChanged(ref _FilteredMechanisms, value); }
        }
        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }
        private List<Mechanism> _mechs = new List<Mechanism>();
        private List<AdditionalData> addDataList = new List<AdditionalData>();

        public Devices_VM(IDataBaseProvider provider, INavigation nav, IJsonSerialization json,
            IDataService service, IAppDbContextFactory appDbContextFactory,
            IFileManagment fileManagment, ITempImage imgMang, Device device = null)
        {
            _service = service;
            _nav = nav;
            _imgManag = imgMang;
            _provider = provider;
            _fileManagment = fileManagment;
            _json = json;
            _appDbContextFactory = appDbContextFactory;
            
            using (var _dbContext = _appDbContextFactory.CreateDbContext())
            {
                var devicesWithDetails = _dbContext.Devices
                    .AsNoTracking()
                    .Include(s => s.Files)
                    .Include(s => s.DeviceType)
                        .ThenInclude(st => st.Characteristics).AsSplitQuery()
                    .ToList();

                Devices = new(devicesWithDetails);

                _deviceTypes = new(devicesWithDetails
                    .Select(s => s.DeviceType!)
                    .Where(st => st != null)
                    .DistinctBy(st => st.Id)
                    .ToList());
                _mechs = _dbContext.Mechanisms.AsNoTracking().Select(x => new Mechanism
                {
                    Id = x.Id,
                    Name = x.Name,
                    SectorID = x.SectorID,
                    MapObjects = new(x.MapObjects.OfType<DeviceAssignment>().Select(x => new DeviceAssignment { DeviceId = x.DeviceId }).ToList())

                }).ToList(); 
                Func<string, Device, bool> filter = (m, p) => p.DeviceType.Name == m;
                DeviceTree = new TreeViewCollection<string, Device>("DeviceType.Name", new(_deviceTypes.Select(x => x.Name).ToList()), Devices, filter);
            }
            SelectedDevice = device;
            LoadAddData();
            this.WhenAnyValue(x => x.SelectedDevice)
                .Where(device => device != null)
                .Select(device => _mechs.Where(mech => mech.MapObjects != null)
                .Where(x => x.MapObjects!.OfType<DeviceAssignment>().Any(da => da.DeviceId == device.Id)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(filteredMechanisms =>
                {
                    FilteredMechanisms = new(filteredMechanisms);
                });
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
            
            _service.WhenAnyValue(x => x.IsEditMode)
                    .BindTo(this, x => x.IsEditMode);
            AddFiles = new RelayCommand<Device>((d) => { fileManagment.AddHelpfulFile(_imgManag,d,true); });
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
                if (!fileManagment.OpenFileInExplorer(file.NameFile))
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
        }

        private void LoadAddData()
        {

            if (File.Exists("DevicesMoreData.json"))
            {
                //заполнение данными из файла
                foreach (var item in _json.ReadFromJsonFile<List<AdditionalData>>("DevicesMoreData.json"))
                {
                    var device = Devices.FirstOrDefault((Func<Device, bool>)(x => x.Name == item.Name));
                    if (device != null)
                    {
                        if (item.HasData())
                        {
                            device.AdditionalData = item;
                        }
                        addDataList.Add(device.AdditionalData);
                    }
                }
            }

            foreach (var deviceType in _deviceTypes.Where(d => d.Characteristics.Any()))
            {
                var currentDevices = Devices.Where(x => x.DeviceTypeId == deviceType.Id);
                foreach (var device in currentDevices)
                {
                    if (device != null && !addDataList.Contains(device.AdditionalData))
                    {
                        device.AdditionalData = AdditionalData.CreateRecord(device.Name, deviceType.Characteristics!);
                        addDataList.Add(device.AdditionalData);
                    }
                    if (addDataList.Contains(device.AdditionalData))
                    {
                        if (device.AdditionalData == null)
                        {
                            device.AdditionalData = AdditionalData.CreateRecord(device.Name, deviceType.Characteristics!);
                            break;
                        }
                        device.AdditionalData.Data = new(deviceType.Characteristics!
                            .Select(c => new MoreData
                            {
                                Parameter = c.Title,
                                Value = device.AdditionalData.Data
                                .FirstOrDefault(d => d.Parameter == c.Title)?.Value ?? string.Empty
                            }).ToList());
                    }
                }

            }
        }
        private void SaveDataFileds()
        {
            string FILE_PATH = "DevicesMoreData.json";
            try
            {
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

        private void UnSetIsNew(IEnumerable<HelpfulFile> files)
        {
            foreach (var item in files)
            {
                item.IsNew = false;
            }
        }
    }
}
