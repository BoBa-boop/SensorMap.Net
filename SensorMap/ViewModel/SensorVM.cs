using CommunityToolkit.Mvvm.Input;
using DynamicData;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using SensorMap.View;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace SensorMap.ViewModel
{
    public class SensorVM:ReactiveObject
    {
        private readonly INavigation _navigation;
        private readonly IDataService _service;
        private readonly IJsonSerialization _json;
        private IAppDbContextFactory _appDbContextFactory;
        private Sensor _sensorsTreeNode;
        private List<AdditionalData> _additionalData;
        private ObservableCollection<Mechanism> _FilteredMechanisms;
        private bool isEditMode;

        [Reactive]public Sensor SelectedNode
        {
            get => _sensorsTreeNode;
            set { this.RaiseAndSetIfChanged(ref _sensorsTreeNode, value); }
        }
        [Reactive]public ObservableCollection<Sensor> Sensors {  get; set; }
        private ObservableCollection<Mechanism> Mechanisms { get; set; }
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
                sensorTypes = new(_dbContext.SensorTypes.AsNoTracking().ToList());
                Sensors = new(_dbContext.Sensors.AsNoTracking().ToList());
                Mechanisms = new(_dbContext.Mechanisms.AsNoTracking().ToList());
                Func<SensorType, Sensor, bool> filter = (type, sensor) => sensor.SensorTypeID == type.Id;
                SensorsTree = new TreeViewCollection<SensorType, Sensor>("Name", sensorTypes, Sensors, filter);
            }
            _additionalData = LoadMoreData();

            SaveMoreData = new RelayCommand(SaveDataFileds);
            NavigateToMech = new RelayCommand<Mechanism>((mech) => 
            {
                if (mech == null) return;
                _service.CurrentMechanism_Global = mech;
                _service.CurrentSector_Global = mech.Sector!;
                _navigation.NavigateTo<MechanismVM>(); 
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
                    var sensor = Sensors.FirstOrDefault(x => x.Name == item.NameSensor);
                    if (sensor != null)
                    {
                        if (item.HasData())
                        {
                            sensor.AdditionalData = item;

                        }
                        else
                        {
                            sensor.AdditionalData = AdditionalData.CreateDefault(sensor.Name);
                        }
                        tempList.Add(sensor.AdditionalData);
                    }
                }
            }
            //заполение доп. данных у датчиков которые не были в файле
            foreach (var sensor in Sensors.Where(s => s.AdditionalData == null || !s.AdditionalData.HasData()))
            {
                sensor.AdditionalData = AdditionalData.CreateDefault(sensor.Name);
                tempList.Add(sensor.AdditionalData);
            }
            return tempList;
        }

        public ICommand SaveMoreData { get; }
        public ICommand NavigateToMech {  get; }
        private void SaveDataFileds()
        {
            string FILE_PATH = "SensorMoreData.json";
            SelectedNode.AdditionalData.NameSensor = SelectedNode.Name;
            var editableObject = _additionalData.Where(x => x.NameSensor == SelectedNode.Name).FirstOrDefault();
            if (editableObject != null)
            {
                editableObject.Data = SelectedNode.AdditionalData.Data;
            }
            else
            {
                _additionalData.Add(SelectedNode.AdditionalData);
            }
            _json.WriteToJsonFile<List<AdditionalData>>(FILE_PATH, _additionalData);
            Growl.Success(new GrowlInfo
            {
                Message = "Дополнительные данные сохранены!",
                CancelStr = "Ignore",
                ShowDateTime = false,
                WaitTime = 2
            });
        }
    }
}
