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
using System.Reactive.Linq;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class SensorVM:ReactiveObject
    {
        private readonly INavigation _navigation;
        private readonly IDataService _service;
        private readonly IJsonSerialization _json;
        private IAppDbContextFactory _appDbContextFactory;
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

        public SensorVM(IDataService service,IJsonSerialization json,INavigation navigation,IAppDbContextFactory appDbContextFactory,Sensor sensor=null)
        {
            SelectedNode = sensor;
            _navigation = navigation;
            _json = json;
            _service = service;
            _appDbContextFactory = appDbContextFactory;
            using (var _dbContext = _appDbContextFactory.CreateDbContext())
            {
                sensorTypes = new(_dbContext.SensorTypes.AsNoTracking().Include(x=>x.Characteristics).ToList());
                Sensors = new(_dbContext.Sensors.Include(x=>x.SensorType).AsNoTracking().ToList());
                Mechanisms = new(_dbContext.Mechanisms.Include(x=>x.SensorsAssig).AsNoTracking().ToList());
                Func<SensorType, Sensor, bool> filter = (type, sensor) => sensor.SensorTypeID == type.Id;
                SensorsTree = new TreeViewCollection<SensorType, Sensor>("Name", sensorTypes, Sensors, filter);
            }
            _additionalData = new(LoadMoreData());

            SaveMoreData = new RelayCommand(SaveDataFileds);
            NavigateToMech = new RelayCommand<Mechanism>((mech) => 
            {
                if (mech == null) return;
                _navigation.NavigateTo<MechanismVM>(mech); 
            });

            this.WhenAnyValue(x => x.SelectedNode)
                .Where(sensor => sensor != null)
                .Select(sensor => Mechanisms.Where(mech =>mech.SensorsAssig!=null)
                .Where(x=>x.SensorsAssig!.Any(sa => sa.SensorId == sensor.Id)))
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
