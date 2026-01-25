using Azure;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Model.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Model
{
    /// <summary>
    /// Описывает участок оборудования
    /// </summary>
    public class Sector : ReactiveObject
    {
        private bool _isModified;
        private string _name = string.Empty;
        private byte[]? _image;
        private ObservableCollection<Mechanism> _mech = new();

        [Key]
        public int Id {  get; set; }
        [MaxLength(100)]
        [Reactive]
        public string Name
        {
            get => _name;
            set
            {
                if(value!=_name)
                {
                    this.RaiseAndSetIfChanged(ref _name, value);
                    IsModified = true;
                }
            }
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
                    IsModified = true;
                }
            }
        }


        public virtual ObservableCollection<Mechanism>? Mechanisms
        {
            get => _mech;
            set
            {
                if (_mech != value)
                {
                    this.RaiseAndSetIfChanged(ref _mech!, value);
                }
            }
        }

        [NotMapped]
        public bool IsModified
        {
            get => _isModified;
            set => this.RaiseAndSetIfChanged(ref _isModified, value);
        }
    }
}

