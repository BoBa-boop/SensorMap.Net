using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SensorMap.Model
{
    /// <summary>
    /// Описывает оборудование
    /// </summary>
    public class Mechanism : ReactiveObject, ICloneable
    {
        private bool _isModified;
        private string _name = string.Empty;
        private byte[]? _image;
        private Device? _device;
        private Sector? sector;
        private ObservableCollection<HelpfulFile> files = new ObservableCollection<HelpfulFile>();

        

        [Key] public int Id { get; set; }
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
        [Reactive] public byte[]? Image
        {
            get => _image;
            set
            {
                if (_image != value)
                {
                    this.RaiseAndSetIfChanged(ref _image, value);
                }
            }
        }
        [Reactive] public virtual Sector? Sector 
        {
            get => sector;
            set
            {
                if (value!=null)
                {
                    this.RaiseAndSetIfChanged(ref sector, value);
                    SectorID = sector!.Id;
                }
            }
        }
        [Reactive] public virtual Device? Device 
        {
            get => _device;
            set
            {
                if(value!=null)
                {
                    this.RaiseAndSetIfChanged(ref _device, value);
                    DeviceID = _device!.Id;
                }
            }
        }

        public virtual ObservableCollection<SensorAssignments>? SensorsAssig { get; set; }
        public virtual ObservableCollection<HelpfulFile> Files { get => files; set => files = value; }
        public virtual int SectorID { get; set; }
        public virtual int? DeviceID { get; set; }


        [NotMapped] public bool IsModified
        {
            get => _isModified;
            set => this.RaiseAndSetIfChanged(ref _isModified, value);
        }

        public object Clone()
        {
            return new Mechanism()
            {
                Id=Id,
                Name=Name,
                Image = Image,
                SectorID = SectorID,
                Sector = Sector,
                DeviceID = DeviceID,
                Device = Device,
                SensorsAssig = new(SensorsAssig),
                Files = Files
            };
        }
    }
}
