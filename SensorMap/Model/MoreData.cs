using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Model
{
    public class AdditionalData : ReactiveObject
    {
        private string _nameSensor = string.Empty;
        private ObservableCollection<MoreData> _data = new();

        [Reactive]
        [JsonProperty("Название датчика")]
        public string NameSensor { get => _nameSensor; set => this.RaiseAndSetIfChanged(ref _nameSensor, value); }
        [Reactive]
        [JsonProperty("Данные")]
        public ObservableCollection<MoreData> Data {
            get => _data;
            set => this.RaiseAndSetIfChanged(ref _data, value); 
        }
        public bool HasData() => Data?.Any() == true;
        public static AdditionalData CreateDefault(string sensorName)
        {
            return new AdditionalData
            {
                NameSensor = sensorName,
                Data = new ObservableCollection<MoreData>
                {
                    new MoreData { Parameter = "Вид Корпуса" },
                    new MoreData { Parameter = "Способ Подключения" },
                    new MoreData { Parameter = "Зона Чувствительности" },
                    new MoreData { Parameter = "Рабочие Напряжения" },
                    new MoreData { Parameter = "Схема Подключения" },
                    new MoreData { Parameter = "Функция Коммутации" },
                }
            };
        }
    }
    public class MoreData:ReactiveObject
    {
        private string _parameter = string.Empty;
        private string _value = string.Empty;

        [Reactive]
        [JsonProperty("Параметр")]
        public string Parameter { get => _parameter; set => this.RaiseAndSetIfChanged(ref _parameter, value); }
        [Reactive]
        [JsonProperty("Значение")]
        public string Value { get => _value; set => this.RaiseAndSetIfChanged(ref _value, value); }

    }
    
}
