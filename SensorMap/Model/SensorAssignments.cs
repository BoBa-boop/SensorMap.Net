using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Model
{
    /// <summary>
    /// Координаты, изображение месторасположния, адрес в ПЛК
    /// </summary>
    public class SensorAssignments : ReactiveObject, ICloneable
    {
        private byte[]? _image;
        private double _y;
        private double _x;
        private double _height;
        private double _width;
        private bool _isModified;
        private bool _isNew;
        private bool _toDelete;

        [Key]
        public int Id { get; set; }
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
        [Reactive]
        public double Width
        {
            get => _width;
            set
            {
                if (value != _width)
                {
                    this.RaiseAndSetIfChanged(ref _width, value);
                }
            }
        }
        [Reactive]
        public double Height
        {
            get => _height;
            set
            {
                if (value != _height)
                {
                    this.RaiseAndSetIfChanged(ref _height, value);
                }
            }
        }
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public virtual int SensorId { get; set; }
        public virtual Sensor? Sensor { get; set; }
        public virtual int MechanismId { get; set; }
        public virtual Mechanism? Mechanism { get; set; }


        [NotMapped]
        public bool IsModified
        {
            get => _isModified;
            set => this.RaiseAndSetIfChanged(ref _isModified, value);
        }
        [NotMapped]
        public bool IsNew
        {
            get => _isNew;
            set => this.RaiseAndSetIfChanged(ref _isNew, value);
        }
        [NotMapped]
        public bool ToDelete
        {
            get => _toDelete;
            set => this.RaiseAndSetIfChanged(ref _toDelete, value);
        }
        public object Clone()
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
    }
}
