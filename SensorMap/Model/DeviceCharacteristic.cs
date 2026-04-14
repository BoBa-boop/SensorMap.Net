using ReactiveUI;
using System.ComponentModel.DataAnnotations;
using ReactiveUI.SourceGenerators;

namespace SensorMap.Model
{
    public class DeviceCharacteristic : ReactiveObject
    {
        [Key] public int Id { get; set; }
        [Reactive] public string Title { get; set; } = string.Empty;
        [Reactive] public int DeviceTypeId { get; set; }
        [Reactive] public virtual DeviceType DeviceType { get; set; } = null!;

    }
}
