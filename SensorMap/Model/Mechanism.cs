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
    public class Mechanism : ReactiveObject, IEditableObject
    {
        private bool _isModified;
        private Mechanism? backupCopy;
        private string _name = string.Empty;
        private byte[]? _image;
        private Sector? sector;
        private PLC? _plc;
        
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
        public int SectorID { get; set; }
        [Reactive]
        public Sector? Sector 
        {
            get => sector;
            set 
            {
                if (sector != value)
                {                    
                    this.RaiseAndSetIfChanged(ref sector, value);
                    IsModified = true;
                }
            } 
        }
        [Reactive]
        public PLC? PLC 
        {
            get => _plc;
            set
            {
                this.RaiseAndSetIfChanged(ref _plc, value);
                IsModified = true;
            }
        }
        public int? PLCID { get; set; }

        

        

        [NotMapped]
        public bool IsModified
        {
            get => _isModified;
            set => this.RaiseAndSetIfChanged(ref _isModified, value);
        }

        public void BeginEdit()
        {
            if (IsModified) return;
            backupCopy = this.MemberwiseClone() as Mechanism;
        }

        public void CancelEdit()
        {
            if (!IsModified) return;
            if (backupCopy == null) return;
            IsModified = false;
            this.Name = backupCopy.Name;
            this.Id = backupCopy.Id;
            this.Image = backupCopy.Image;
            this.Path = backupCopy.Path;
        }

        public void EndEdit()
        {

        }

       
    }
}
