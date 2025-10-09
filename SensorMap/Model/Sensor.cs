using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reactive.Linq;

namespace SensorMap.Model
{
    /// <summary>
    /// Описывает датчик (тип, картинка, название)
    /// </summary>
    public class Sensor:ReactiveObject
    {
        private bool _isInitialized = false;
        private string _name = string.Empty;
        private SensorType _type;
        private string _image = string.Empty;
        [Key]
        [Reactive] public int Id { get; set; }
        [Reactive] public string Name 
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        [Reactive] public SensorType Type
        {
            get => _type;
            set => this.RaiseAndSetIfChanged(ref _type, value);
        }
        [Reactive]
        public string Image
        {
            get => _image;
            set => this.RaiseAndSetIfChanged(ref _image, value);
        }

        private bool _isModified;
        [NotMapped]
        public bool IsModified
        {
            get => _isModified;
            set => this.RaiseAndSetIfChanged(ref _isModified, value);
        }
        public Sensor()
        {
            SetupPropertyChangeTracking();
        }
        private void SetupPropertyChangeTracking()
        {
            this.WhenAnyValue(
                x => x.Name,
                x => x.Type,
                x => x.Image)
                .Skip(1)
                .Subscribe(_ => IsModified = true);
        }
    }
}
