using ReactiveUI;
using ReactiveUI.SourceGenerators;
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
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;//путь до папки с данными
        public string Image { get; set; } = string.Empty;
        public int SectorID { get; set; }
        public Sector? Sector { get; set; }//ссылка для EF
        public PLC? PLC { get; set; }//ссылка для EF

        private bool _isModified;
        private Mechanism? backupCopy;

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
