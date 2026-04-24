using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Model
{
    public class SensorType:ReactiveObject
    {
        private string _name = string.Empty;
        private string? _color;
        private bool _isNew;

        [Key]
        [Reactive] public int Id { get; set; }
        [MaxLength(30)]
        [Reactive] public string Name
        {
            get => _name;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.RaiseAndSetIfChanged(ref _name, value);
                    IsNew = true;
                }
            } 
        }
        [Reactive] public string? Color
        {
            get => _color;
            set
            {
                if (value != _color)
                {
                    this.RaiseAndSetIfChanged(ref _color, value);
                    IsNew = true;
                }
            }
        }

        [NotMapped]
        [Reactive]
        public bool IsNew
        {
            get => _isNew;
            set
            {
                this.RaiseAndSetIfChanged(ref _isNew, value);
            }
        }
        
        public virtual ObservableCollection<Sensor>? Sensors { get; set; }
        public virtual ObservableCollection<SensorCharacteristic>? Characteristics { get; set; } = new ObservableCollection<SensorCharacteristic>();

    }
}
