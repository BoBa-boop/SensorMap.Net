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
        private readonly AppDBContext _dbContext;
        private readonly IFileManagment _fileManagment;
        private IAppDbContextFactory _appDbContextFactory;
        private Device _selectedDevice;
        private ObservableCollection<Mechanism> _FilteredMechanisms;
        private bool isEditMode;
        [Reactive] public Device SelectedDevice
        {
            get => _selectedDevice;
            set { this.RaiseAndSetIfChanged(ref _selectedDevice, value); }
        }
        [Reactive] public ObservableCollection<Device> Device { get; set; }
        private ObservableCollection<DeviceType> _deviceType { get; set; }
        [Reactive] public TreeViewCollection<string, Device> DeviceTree { get; set; }
        [Reactive] public ObservableCollection<Mechanism> FilteredMechanisms
        {
            get => _FilteredMechanisms;
            set { this.RaiseAndSetIfChanged(ref _FilteredMechanisms, value); }
        }
        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }
        private ObservableCollection<Mechanism> _mechs = new ObservableCollection<Mechanism>();
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
            _dbContext = _appDbContextFactory.CreateDbContext();
            Device = new(_dbContext.Devices.Include(x => x.DeviceType).ThenInclude(k => k.Characteristics)
                                           .Include(k=>k.Files).AsSplitQuery().ToList());
            _mechs = new(_dbContext.Mechanisms.ToList());
            _deviceType = new(_dbContext.DeviceTypes.ToList());
            
            SelectedDevice = device;
            LoadAddData();
            this.WhenAnyValue(x => x.SelectedDevice)
                .Where(device => device != null)
                .Select(device => _mechs.Where(mech => mech.Device != null)
                .Where(x => x.DeviceID == device.Id))
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
            SaveMoreData = new RelayCommand(SaveDataFileds);
            Func<string, Device, bool> filter = (m, p) => p.DeviceType.Name == m;
            DeviceTree = new TreeViewCollection<string, Device>("DeviceType.Name", new(_deviceType.Select(x => x.Name).ToList()), Device, filter);
            _service.WhenAnyValue(x => x.IsEditMode)
                    .BindTo(this, x => x.IsEditMode);
            AddFiles = new RelayCommand<Device>((d) =>
            {
                string[] paths = _fileManagment.OpenFileDialog();
                foreach (var path in paths)
                {
                    d.Files.Add(new HelpfulFile()
                    {
                        NameFile = path,
                        ImageFile = _imgManag.ConvertToByte(_fileManagment.GetIconFile(path))
                    });
                }
            });
            DeletePathFiles = new RelayCommand<HelpfulFile>((file) =>
            {
                SelectedDevice.Files.Remove(file);
                try
                {

                    //_dbContext.Remove(SelectedNode);
                    _dbContext.SaveChanges();
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
            });
            OpenFile = new RelayCommand<HelpfulFile>((file) =>
            {
                Process.Start("explorer.exe", "/select,\""+Path.GetFullPath(file.NameFile) + "\"");
            });
            SaveFiles = new RelayCommand(() =>
            {
                try
                {
                    if (_dbContext.ChangeTracker.HasChanges())
                    {
                        //_dbContext.Update(SelectedNode);
                        _dbContext.SaveChanges();
                        Growl.Success(new GrowlInfo
                        {
                            Message = "Путь к файлам сохранен.",
                            CancelStr = "Ignore",
                            ShowDateTime = false,
                            WaitTime = 2
                        });
                    }
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
                    var device = Device.FirstOrDefault((Func<Device, bool>)(x => x.Name == item.Name));
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

            foreach (var deviceType in _deviceType.Where(d => d.Characteristics.Any()))
            {
                var currentDevices = Device.Where(x => x.DeviceTypeId == deviceType.Id);
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

    }
}
