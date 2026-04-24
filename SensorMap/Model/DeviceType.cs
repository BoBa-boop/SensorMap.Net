using ReactiveUI;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReactiveUI.SourceGenerators;

namespace SensorMap.Model
{
    public class DeviceType:ReactiveObject
    {
        private string _name;
        private bool _isNew;

        [Key]
        [Reactive] public int Id { get; set; }
        [MaxLength(30)]
        [Reactive]
        public string Name
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

        public virtual ObservableCollection<Device>? Devices { get; set; }
        public virtual ObservableCollection<DeviceCharacteristic>? Characteristics { get; set; } = new ObservableCollection<DeviceCharacteristic>();
    }
}
