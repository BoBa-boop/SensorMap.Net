using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reactive.Linq;
using System.Windows.Input;

namespace SensorMap.Model
{
    /// <summary>
    /// Описывает датчик (тип, картинка, название)
    /// </summary>
    public class Sensor : ReactiveObject
    {
        private string _name = string.Empty;
        private bool _isModified;
        private SensorType? _sensorType;
        private byte[]? _image;

        [Key]
        [Reactive] public int Id { get; set; }
        [MaxLength(250)]
        [Reactive] public string Name 
        {
            get => _name;
            set  
            {
                if (_name != value)
                {
                    this.RaiseAndSetIfChanged(ref _name, value);
                }
            }
        }
        public int SensorTypeID { get; set; }
        [Reactive]
        public byte[]? Image
        {
            get => _image;
            set 
            {
                if (value != _image)
                {
                    this.RaiseAndSetIfChanged(ref _image, value);
                }
            }
        }
        public SensorType? SensorType
        {
            get => _sensorType;
            set
            {
                if (value != null)
                {
                    this.RaiseAndSetIfChanged(ref _sensorType, value);
                }
            }
        }
        [NotMapped]
        public bool IsModified
        {
            get => _isModified;
            set => this.RaiseAndSetIfChanged(ref _isModified, value);
        }

        [NotMapped] public AdditionalData AdditionalData { get; set; }
    }
}
