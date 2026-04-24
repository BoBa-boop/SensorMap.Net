using Newtonsoft.Json.Linq;
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
    /// Описывает устройство
    /// </summary>
    public class Device:ReactiveObject
    {
        private bool _isModified;
        private byte[]? _image;
        private string _name = string.Empty;
        private string _manufacturer = string.Empty;
        private DeviceType? _type;

        [Key]public int Id { get; set; }
        [Reactive] public byte[]? Image
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
        [Reactive] public ObservableCollection<Device>? ChildrenDevices { get; set; }
        public Device? MasterDevice { get; set; }
        public int? MasterDeviceID { get; set; }
        public virtual ObservableCollection<Mechanism>? Mechanisms { get; set; }
        public virtual ObservableCollection<HelpfulFile> Files { get; set; }
        public int? DeviceTypeId {  get; set; }
        public DeviceType? DeviceType
        { 
            get => _type;
            set
            {
                if (value != null)
                {
                    this.RaiseAndSetIfChanged(ref _type, value);
                    DeviceTypeId = _type!.Id;
                }
            }
        }
[NotMapped] public bool IsModified
        {
            get => _isModified;
            set => this.RaiseAndSetIfChanged(ref _isModified, value);
        }
        [NotMapped] public AdditionalData? AdditionalData { get; set; }
    }
}
