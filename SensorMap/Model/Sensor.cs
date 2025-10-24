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
    public class Sensor : ReactiveObject, IEditableObject, IDataErrorInfo
    {
        private string _name = string.Empty;
        private SensorType _type;
        private byte[]? _image;
        [Key]
        [Reactive] public int Id { get; set; }
        [MaxLength(250)]
        [Reactive] public string Name 
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
        [Reactive] public SensorType Type
        {
            get => _type;
            set 
            {
                if (value != _type)
                {
                    this.RaiseAndSetIfChanged(ref _type, value);
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
                }
            }
        }

        private bool _isModified;
        [NotMapped]
        public bool IsModified
        {
            get => _isModified;
            set => this.RaiseAndSetIfChanged(ref _isModified, value);
        }

        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "Name":
                        if (string.IsNullOrWhiteSpace(_name))
                        {
                            IsModified = false;
                            return "Обязательное поле к заполнению!";
                        }
                        break;
                }
                return string.Empty;
            }
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
            //if (!IsModified) return;
            //if (backupCopy == this.MemberwiseClone())
            //{
            //    IsModified = false;
            //    backupCopy = null;
            //}

        }
    }
}
