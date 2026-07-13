using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.CustomControls;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows;
using System.Windows.Controls;

namespace SensorMap.Model
{
    public class DeviceAssignment:MapObject
    /// <summary>
    /// Размещение устройства на карте Mechanism
    /// </summary>
    public class DeviceAssignment : MapObject
    {
        private Device? _device;

        public virtual int DeviceId { get; set; }

        [Reactive]
        public virtual Device? Device
        {
            get => _device;
            set
            {
                if (_device != value)
                {
                    this.RaiseAndSetIfChanged(ref _device, value);
                    DeviceId = value?.Id ?? 0;
                }
            }
        }

        public override object Clone()
        {
            return new DeviceAssignment
            {
                Id = Id,
                DeviceId = DeviceId,
                Device = Device,
                MechanismId = MechanismId,
                Mechanism = Mechanism,
                Description = Description,
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
            return canvas.Children.OfType<CustomDevice>()
                .FirstOrDefault(s => s.DeviceData.Id == this.Id);
        }
    }
}
