using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Model
{
    /// <summary>
    /// Тип для входного сигнала в ПЛК
    /// </summary>
    public class PLCInputs:ReactiveObject
    {
        [Reactive] public int Id {  get; set; }
        [Reactive] public Sensor? Sensor { get; set; }
        [Reactive] public PLC? PLC { get; set; }
        [Reactive] public Mechanism? Mechanism { get; set; }

        [Reactive] public string Address { get; set; } = string.Empty;
        [Reactive] public string Description { get; set; } = string.Empty;
        [Reactive] public string Location { get; set; } = string.Empty; 
        [Reactive] public double XPoint { get; set; }
        [Reactive] public double YPoint { get; set; }
    }
}
