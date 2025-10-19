using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Model
{
    /// <summary>
    /// Описывает участок оборудования
    /// </summary>
    public class Sector : ReactiveObject, IEditableObject
    {
        [Key]
        public int Id {  get; set; }
        [MaxLength(100)]
        public string Name {  get; set; } = string.Empty;
        public ObservableCollection<Mechanism>? Mechanisms { get; set; }
        [Column(TypeName = "image")]
        public byte[] Image { get; set; }

        private bool _isModified;
        private Sector? backupCopy;

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

