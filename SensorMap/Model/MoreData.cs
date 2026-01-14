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
    
        //private string видКорпуса = string.Empty;
        //private string способПодключения = string.Empty;
        //private string зонаЧувствительности = string.Empty;
        //private string рабочиеНапряжения = string.Empty;
        //private string схемаПодключения = string.Empty;
        //private string функцияКоммутации = string.Empty;

        
        //[Reactive] public string ВидКорпуса { get => видКорпуса; set => this.RaiseAndSetIfChanged(ref видКорпуса, value); }
        //[Reactive] public string СпособПодключения { get => способПодключения; set => this.RaiseAndSetIfChanged(ref способПодключения, value); }
        //[Reactive] public string ЗонаЧувствительности { get => зонаЧувствительности; set => this.RaiseAndSetIfChanged(ref зонаЧувствительности, value); }
        //[Reactive] public string РабочиеНапряжения { get => рабочиеНапряжения; set => this.RaiseAndSetIfChanged(ref рабочиеНапряжения, value); }
        //[Reactive] public string СхемаПодключения { get => схемаПодключения; set => this.RaiseAndSetIfChanged(ref схемаПодключения, value); }
        //[Reactive] public string ФункцияКоммутации { get => функцияКоммутации; set => this.RaiseAndSetIfChanged(ref функцияКоммутации, value); }

    }
