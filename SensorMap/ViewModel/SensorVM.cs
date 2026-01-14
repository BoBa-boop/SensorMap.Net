using CommunityToolkit.Mvvm.Input;
using DynamicData;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using SensorMap.View;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace SensorMap.ViewModel
{
    public class SensorVM:ReactiveObject
    {
        private readonly IDataService _service;
        private readonly IJsonSerialization _json;
        private Sensor _sensorsTreeNode;
        private List<AdditionalData> _additionalData;
        [Reactive]public Sensor SelectedNode
        {
            get => _sensorsTreeNode;
            set { this.RaiseAndSetIfChanged(ref _sensorsTreeNode, value); }
        }
        [Reactive]public ObservableCollection<Sensor> Sensors {  get; set; }
        
        [Reactive] public TreeViewCollection<SensorType, Sensor> SensorsTree { get; set; }
        private ObservableCollection<SensorType> sensorTypes {  get; set; }

        public SensorVM(IDataService service,IJsonSerialization json,Sensor sensor=null)
        {
            SelectedNode = sensor;
            _json = json;
            _service = service;
            sensorTypes = _service.SensorTypes;
            Sensors = _service.Sensors;
            Func<SensorType, Sensor, bool> filter = (type, sensor) => sensor.SensorTypeID == type.Id;
            SensorsTree = new TreeViewCollection<SensorType, Sensor>("Name", sensorTypes, Sensors, filter);
            _additionalData = LoadMoreData();

            SaveMoreData = new RelayCommand(SaveDataFileds);
        }

        private List<AdditionalData> LoadMoreData()
        {
            List<AdditionalData> tempList = new List<AdditionalData>();
            if (File.Exists("SensorMoreData.json"))
                foreach (var item in _json.ReadFromJsonFile<List<AdditionalData>>("SensorMoreData.json"))
                {
                    var element = Sensors.FirstOrDefault(x => x.Name == item.NameSensor);
                    if (element != null)
                    {
                        element.AdditionalData.Data = item.Data;
                        tempList.Add(item);
                    }
                }
            if (tempList.Count == 0)
            {
                //временный способ заполнение базовыми параметрами
                foreach (var item in Sensors)
                {
                    item.AdditionalData = new AdditionalData()
                    {
                        Data = new ObservableCollection<MoreData>()
                        {
                            new MoreData {Parameter = "Вид Корпуса" },
                            new MoreData { Parameter = "Способ Подключения" },
                            new MoreData { Parameter = "Зона Чувствительности" },
                            new MoreData { Parameter = "Рабочие Напряжения" },
                            new MoreData { Parameter = "Схема Подключения" },
                            new MoreData { Parameter = "Функция Коммутации" },
                        }
                    };
                }
            }
            return tempList;

        }

        public ICommand SaveMoreData { get; }
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
                try
                {
                    _json.WriteToJsonFile<List<AdditionalData>>(FILE_PATH, _additionalData);
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

                }

        }
    }
}
