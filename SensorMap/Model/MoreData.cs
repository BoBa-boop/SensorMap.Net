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
        private string _name = string.Empty;
        private ObservableCollection<MoreData> _data = new();

        [Reactive]
        [JsonProperty("Название")]
        public string Name { get => _name; set => this.RaiseAndSetIfChanged(ref _name, value); }
        [Reactive]
        [JsonProperty("Данные")]
        public ObservableCollection<MoreData> Data {
            get => _data;
            set => this.RaiseAndSetIfChanged(ref _data, value); 
        }
        public bool HasData() => Data?.Any() == true;
        public static AdditionalData CreateRecord(string name, IEnumerable<SensorCharacteristic> characteristics)
        {
            var record = new AdditionalData() { Name = name };
            var data = new ObservableCollection<MoreData>();
            foreach (var param in characteristics)
            {
                data.Add(new MoreData() { Parameter = param.Title });
            }
            record.Data = data;
            return record;
        }
        public static AdditionalData CreateRecord(string name, IEnumerable<DeviceCharacteristic> characteristics)
        {
            var record = new AdditionalData() { Name = name };
            var data = new ObservableCollection<MoreData>();
            foreach (var param in characteristics)
            {
                data.Add(new MoreData() { Parameter = param.Title });
            }
            record.Data = data;
            return record;
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
