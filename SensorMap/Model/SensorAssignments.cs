using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Model
{
    /// <summary>
    /// Координаты, изображение месторасположния, адрес в ПЛК
    /// </summary>
    public class SensorAssignments:ReactiveObject
    {
        private byte[]? _image;
        private double _y;
        private double _x;

        [Key]
        public int Id { get; set; }
        [Reactive]
        public byte[]? LocationImage
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
        [Reactive]
        public double X
        {
            get => _x;
            set
            {
                if (value != _x)
                {
                    this.RaiseAndSetIfChanged(ref _x, value);
                }
            }
        }
        [Reactive]
        public double Y
        {
            get => _y;
            set
            {
                if (value != _y)
                {
                    this.RaiseAndSetIfChanged(ref _y, value);
                }
            }
        }
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public virtual int SensorId { get; set; }
        public virtual Sensor? Sensor { get; set; }
        public virtual int MechanismId { get; set; }
        public virtual Mechanism? Mechanism { get; set; }
    }
}
