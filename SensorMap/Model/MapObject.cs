using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows;
using System.Windows.Controls;
namespace SensorMap.Model
{
    public abstract class MapObject : ReactiveObject, ICloneable
    {
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private string _description = string.Empty;
        private byte[]? _image;
        private bool _isModified;
        private bool _isNew;
        private bool _toDelete;

        [Key]
        public int Id { get; set; }

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

        [Reactive]
        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

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

        public abstract object Clone();
        public abstract UIElement FindInCanvas(Canvas canvas);
    }
}
