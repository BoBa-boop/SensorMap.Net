using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SensorMap.Model
{
    /// <summary>
    /// Описывает датчик (тип, картинка, название)
    /// </summary>
    public class Sensor:ReactiveObject
    {
        [Key]
        [Reactive] public int Id { get; set; }
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public SensorType Type { get; set; }
        [Reactive] public string Image { get; set; } = string.Empty;

        private bool _isModified;
        [NotMapped]
        public bool IsModified
        {
            get => _isModified;
            set => this.RaiseAndSetIfChanged(ref _isModified, value);
        }
        public Sensor CreateSnapshot()
        {
            return new Sensor
            {
                Id = Id,
                Name = Name,
                Type = Type,
                Image = Image,
                IsModified = IsModified
            };
        }
        public void RestoreFromSnapshot(Sensor snapshot)
        {
            Name = snapshot.Name;
            Type = snapshot.Type;
            Image = snapshot.Image;
            IsModified = snapshot.IsModified;
        }
    }
}
