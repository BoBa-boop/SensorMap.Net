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
        [Reactive] public int Id { get; set; }
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string Path { get; set; } = string.Empty;//путь до папки с данными
        [Reactive] public ObservableCollection<PLCInputs> Sensors { get; set; }
        [Reactive] public Sector? Sector { get; set; }//участок за которым закреплено оборудование
        [Reactive] public string Image { get; set; } = string.Empty;
        [Reactive]public PLC? PLC { get; set; }

    }
}
