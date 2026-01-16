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
