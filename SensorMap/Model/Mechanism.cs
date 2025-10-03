using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;

namespace SensorMap.Model
{
    /// <summary>
    /// Описывает оборудование
    /// </summary>
    public class Mechanism : ReactiveObject
    {
        [Reactive] public int ID { get; set; }
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string Path { get; set; } = string.Empty;//путь до папки с данными
        [Reactive] public string Image { get; set; } = string.Empty;

        [Reactive] public ObservableCollection<PLCInputs> Sensors { get; set; }
        [Reactive]public PLC? PLC { get; set; }

    }
}
