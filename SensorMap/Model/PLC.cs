using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Model
{
    /// <summary>
    /// Описывает контроллер
    /// </summary>
    public class PLC:ReactiveObject
    {
        [Reactive] public int Id {  get; set; }
        [Reactive] public string TypePLC { get; set; } = string.Empty;
        [Reactive] public string Image { get; set; } = string.Empty;
        [Reactive] public string IP { get; set; } = string.Empty;
        [Reactive] public ObservableCollection<PLCInputs>? Inputs { get; set; }
    }
}
