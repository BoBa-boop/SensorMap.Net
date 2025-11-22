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
    /// <summary>
    /// Описывает контроллер
    /// </summary>
    public class PLC:ReactiveObject
    {
        private bool _isModified;
        private byte[]? _image;
        private string _name = string.Empty;
        private string _ip;

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
                    IsModified = true;
                }
            }
        }
        [MaxLength(15)]
        public string? IP
        {
            get => _ip;
            set
            {
                if (_ip != value)
                {
                    this.RaiseAndSetIfChanged(ref _ip, value);
                }
            }
        }
        [Reactive]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    this.RaiseAndSetIfChanged(ref _name, value);
                    IsModified = true;
                }
            }
        }
        public string? Manufacturer { get; set; } = string.Empty;
        public ObservableCollection<PLCInputs>? Inputs { get; set; }
        public ObservableCollection<Mechanism>? Mechanisms { get; set; }
        [NotMapped]
        public bool IsModified
        {
            get => _isModified;
            set => this.RaiseAndSetIfChanged(ref _isModified, value);
        }
    }
}
