using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.CustomControls;
using System.Windows;
using System.Windows.Controls;

namespace SensorMap.Model
{
    /// <summary>
    /// Размещение датчика на карте Mechanism. Координаты, адрес в ПЛК, изображение.
    /// </summary>
    public class SensorAssignments : MapObject
    {
        private string _address = string.Empty;
        private Sensor? _sensor;

        [Reactive]
        public string Address
        {
            get => _address;
            set => this.RaiseAndSetIfChanged(ref _address, value);
        }

        public virtual int SensorId { get; set; }

        [Reactive]
        public virtual Sensor? Sensor
        {
            get => _sensor;
            set
            {
                if (_sensor != value)
                {
                    this.RaiseAndSetIfChanged(ref _sensor, value);
                    SensorId = value?.Id ?? 0;
                }
            }
        }

        public override object Clone()
        {
            return new SensorAssignments
            {
                Id = Id,
                SensorId = SensorId,
                MechanismId = MechanismId,
                Address = Address,
                Description = Description,
                Sensor = Sensor,
                Mechanism = Mechanism,
                X = X,
                Y = Y,
                Width = Width,
                Height = Height,
                Image = Image,
                IsModified = IsModified
            };
        }
        public override UIElement FindInCanvas(Canvas canvas)
        {
            return canvas.Children.OfType<CustomSensor>()
                .FirstOrDefault(s => s.SensorData.Id == this.Id);
        }
    }
}
