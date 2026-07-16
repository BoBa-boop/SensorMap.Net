using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.EntityFrameworkCore;
using NLog;
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

namespace SensorMap.ViewModel
{
    public class SensorVM:ReactiveObject
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly INavigation _navigation;
        private readonly IDataService _service;
        private readonly IJsonSerialization _json;
        private readonly ITempImage imgManag;
        private IAppDbContextFactory _appDbContextFactory;
        private readonly IFileManagment _fileManagment;
        private readonly AppDBContext _dbContext;
        private Sensor _sensorsTreeNode;
        public ObservableCollection<AdditionalData> _additionalData;
        private ObservableCollection<Mechanism> Mechanisms { get; set; }
        private ObservableCollection<Mechanism> _FilteredMechanisms;
        private ObservableCollection<SensorCharacteristic> _sensorCharacteristics;

        private bool isEditMode;

        [Reactive]public Sensor SelectedNode
        {
            get => _sensorsTreeNode;
            set { this.RaiseAndSetIfChanged(ref _sensorsTreeNode, value); }
        }
        [Reactive]public ObservableCollection<Sensor> Sensors {  get; set; }
        
        [Reactive] public ObservableCollection<Mechanism> FilteredMechanisms 
        {
            get => _FilteredMechanisms;
            set { this.RaiseAndSetIfChanged(ref _FilteredMechanisms, value); }
        }
        [Reactive] public TreeViewCollection<SensorType, Sensor> SensorsTree { get; set; }
        private ObservableCollection<SensorType> sensorTypes {  get; set; }
        
        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }

        public SensorVM(IDataService service, IJsonSerialization json,
            ITempImage imgManag,
            INavigation navigation, IAppDbContextFactory appDbContextFactory,
            IFileManagment fileManagment, Sensor sensor = null)
        {
            SelectedNode = sensor;
            _navigation = navigation;
            _json = json;
            this.imgManag = imgManag;
            _service = service;
            _appDbContextFactory = appDbContextFactory;
            _fileManagment = fileManagment;
            _dbContext = _appDbContextFactory.CreateDbContext();
            
                sensorTypes = new(_dbContext.SensorTypes.AsNoTracking().Include(x => x.Characteristics).ToList());
                Sensors = new(_dbContext.Sensors.Include(s=>s.Files).ToList());
                Mechanisms = new(_dbContext.Mechanisms.Include(x => x.MapObjects).AsNoTracking().ToList());
                Func<SensorType, Sensor, bool> filter = (type, sensor) => sensor.SensorTypeID == type.Id;
                SensorsTree = new TreeViewCollection<SensorType, Sensor>("Name", sensorTypes, Sensors, filter);
            
            _additionalData = new(LoadMoreData());

            SaveMoreData = new RelayCommand(SaveDataFileds);
            NavigateToMech = new RelayCommand<Mechanism>((mech) =>
            {
                if (mech == null) return;
                _navigation.NavigateTo<MechanismVM>(mech);
            });
            AddFiles = new RelayCommand<Sensor>((s) => { fileManagment.AddHelpfulFile(imgManag, s,true);  });
            DeletePathFiles = new RelayCommand<HelpfulFile>((file) =>
            {
                SelectedNode.Files.Remove(file);
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
            OpenFile = new RelayCommand<HelpfulFile>((file) => { fileManagment.OpenFileInExplorer(file.NameFile); });
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

            this.WhenAnyValue(x => x.SelectedNode)
                .Where(sensor => sensor != null)
                .Select(sensor => Mechanisms.Where(mech =>mech.MapObjects!=null)
                .Where(x=>x.MapObjects!.OfType<SensorAssignments>().Any(sa => sa.SensorId == sensor.Id)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(filteredMechanisms =>
                { 
                    FilteredMechanisms = new(filteredMechanisms); 
                });
            
            _service.WhenAnyValue(x => x.IsEditMode)
                .BindTo(this, x => x.IsEditMode);
        }

        private List<AdditionalData> LoadMoreData()
        {
            List<AdditionalData> tempList = new List<AdditionalData>();
            if (File.Exists("SensorMoreData.json"))
            {
                //заполнение данными из файла
                foreach (var item in _json.ReadFromJsonFile<List<AdditionalData>>("SensorMoreData.json"))
                {
                    var sensor = Sensors.FirstOrDefault(x => x.Name == item.Name);
                    if (sensor != null)
                    {
                        if (item.HasData())
                        {
                            sensor.AdditionalData = item;
                        }
                        tempList.Add(sensor.AdditionalData);
                    }
                }
            }

            foreach (var sensorType in sensorTypes.Where(s => s.Characteristics.Any()))
            {
                var SensorsOneType = Sensors.Where(x => x.SensorTypeID == sensorType.Id);
                foreach (var sensor in SensorsOneType)
                {
                    if (sensor != null && !tempList.Contains(sensor.AdditionalData))
                    {
                        sensor.AdditionalData = AdditionalData.CreateRecord(sensor.Name, sensorType.Characteristics!);
                        tempList.Add(sensor.AdditionalData);
                    }
                    if (tempList.Contains(sensor.AdditionalData))
                    {
                        if(sensor.AdditionalData == null)
                        {
                            sensor.AdditionalData = AdditionalData.CreateRecord(sensor.Name, sensorType.Characteristics!);
                            break;
                        }
                        sensor.AdditionalData.Data = new (sensorType.Characteristics!
                            .Select(c => new MoreData
                            {
                                Parameter = c.Title, 
                                Value = sensor.AdditionalData.Data
                                .FirstOrDefault(d => d.Parameter == c.Title)?.Value ?? string.Empty 
                            }).ToList());
                    }
                }
               
            }
            return tempList;
        }

        public ICommand SaveMoreData { get; }
        public ICommand AddFiles { get; }
        public ICommand DeletePathFiles { get; }
        public ICommand SaveFiles { get; }
        public ICommand OpenFile { get; }
        public ICommand NavigateToMech {  get; }
        private void SaveDataFileds()
        {
            string FILE_PATH = "SensorMoreData.json";
            try
            {
                SelectedNode.AdditionalData.Name = SelectedNode.Name;
                var editableObject = _additionalData.Where(x => x.Name == SelectedNode.Name).FirstOrDefault();
                if (editableObject != null)
                {
                    editableObject.Data = SelectedNode.AdditionalData.Data;
                }
                else
                {
                    _additionalData.Add(SelectedNode.AdditionalData);
                }
                _json.WriteToJsonFile<ObservableCollection<AdditionalData>>(FILE_PATH, _additionalData);
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

    }
}
