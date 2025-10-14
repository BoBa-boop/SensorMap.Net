using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reactive.Linq;
using System.Windows.Input;

namespace SensorMap.Model
{
    /// <summary>
    /// Описывает датчик (тип, картинка, название)
    /// </summary>
    public class Sensor : ReactiveObject, IEditableObject
    {
        private string _name = string.Empty;
        private SensorType _type;
        private string _image = string.Empty;
        [Key]
        [Reactive] public int Id { get; set; }
        [Reactive] public string Name 
        {
            get => _name;
            set  { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        [Reactive] public SensorType Type
        {
            get => _type;
            set { this.RaiseAndSetIfChanged(ref _type, value); }
        }
        [Reactive]
        public string Image
        {
            get => _image;
            set { this.RaiseAndSetIfChanged(ref _image, value); }
        }

        private bool _isModified;
        [NotMapped]
        public bool IsModified
        {
            get => _isModified;
            set => this.RaiseAndSetIfChanged(ref _isModified, value);
        }
        private bool _isExist;
        [NotMapped]
        public bool IsExist
        {
            get => _isExist;
            set => this.RaiseAndSetIfChanged(ref _isExist, value);
        }
        public Sensor()
        {
           
        }


        private Sensor backupCopy;
        public void BeginEdit()
        {
            if (IsModified) return;
            backupCopy = this.MemberwiseClone() as Sensor;
        }

        public void CancelEdit()
        {
            if (!IsModified) return;
            if (backupCopy == null) return;
            IsModified = false;
            this.Name = backupCopy.Name;
            this.Id = backupCopy.Id;
            this.Image = backupCopy.Image;
            this.Type = backupCopy.Type;
        }

        public void EndEdit()
        {
            
        }
    }
}
