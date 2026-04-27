using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.ComponentModel.DataAnnotations;

namespace SensorMap.Model
{
    public class HelpfulFile:ReactiveObject
    {
        private string nameFile = string.Empty;
        private byte[]? imageFile;
        private int id;

        [Key]public int Id { get => id; set => id = value; }
        [Reactive]public string NameFile { get => nameFile; set => this.RaiseAndSetIfChanged(ref nameFile,value); }
        [Reactive]public byte[] ImageFile { get => imageFile; set => this.RaiseAndSetIfChanged(ref imageFile, value); }
        public int? SensorId { get; set; }
        public Sensor Sensor { get; set; }
        public int? DeviceId { get; set; }
        public Device Device { get; set; }
        public Mechanism Mechanism { get; set; }
        public int? MechanismId { get; set; }

    }
}
