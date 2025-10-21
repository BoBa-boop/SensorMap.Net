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
    public class Sector : ReactiveObject,IEditableObject
    {
        [Key]
        public int Id {  get; set; }
        [MaxLength(100)]
        [Reactive]
        public string Name
        {
            get => _name;
            set
            {
                this.RaiseAndSetIfChanged(ref _name, value);
                IsModified = true;
            }
        }
        public ObservableCollection<Mechanism>? Mechanisms { get; set; }
        [Reactive]
        public byte[]? Image
        {
            get => _image;
            set 
            {
                this.RaiseAndSetIfChanged(ref _image, value);
                IsModified = true;
            }
        }
        private bool _isModified;
        private Sector? backupCopy;
        private string _name;
        private byte[]? _image;

        [NotMapped]
        public bool IsModified
        {
            get => _isModified;
            set => this.RaiseAndSetIfChanged(ref _isModified, value);
        }

        public void BeginEdit()
        {
            if (IsModified) return;
            backupCopy = this.MemberwiseClone() as Sector;
        }

        public void CancelEdit()
        {
            if (!IsModified) return;
            if (backupCopy == null) return;
            IsModified = false;
            this.Name = backupCopy.Name;
            this.Id = backupCopy.Id;
        }

        public void EndEdit()
        {

        }
    }
}

