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
    public class Mechanism : ReactiveObject
    {
        private bool _isModified;
        private string _name = string.Empty;
        private byte[]? _image;
        private PLC? _plc;
        private Sector? sector;

        [Key]
        public int Id { get; set; }
        [MaxLength(250)]
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
        public string Path { get; set; } = string.Empty;//путь до папки с данными
        
        [Reactive]
        public byte[]? Image
        {
            get => _image;
            set
            {
                if (_image != value)
                {
                    this.RaiseAndSetIfChanged(ref _image, value);
                    IsModified = true;
                }
            }
        }
        public virtual int SectorID { get; set; }
        public virtual int? PLCID { get; set; }

        [Reactive]
        public virtual Sector? Sector 
        {
            get => sector;
            set
            {
                if (value!=null)
                {
                    this.RaiseAndSetIfChanged(ref sector, value);
                    SectorID = sector!.Id;
                    IsModified = true;
                }
            }
        }
        [Reactive]
        public virtual PLC? PLC 
        {
            get => _plc;
            set
            {
                if(value!=null)
                {
                    this.RaiseAndSetIfChanged(ref _plc, value);
                    PLCID = _plc!.Id;
                    IsModified = true;
                }
            }
        }
        public virtual ObservableCollection<SensorAssignments>? SensorsAssig { get; set; }

        

        [NotMapped]
        public bool IsModified
        {
            get => _isModified;
            set => this.RaiseAndSetIfChanged(ref _isModified, value);
        }
    }
}
