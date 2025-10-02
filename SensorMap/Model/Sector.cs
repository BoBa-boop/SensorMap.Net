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
    /// Описывает участок оборудования
    /// </summary>
    public class Sector:ReactiveObject
    {
        [Reactive] public int Id {  get; set; }
        [Reactive] public string Name {  get; set; } = string.Empty;
        
        [Reactive] public ObservableCollection<Mechanism> Mechanisms { get; set; } = new ObservableCollection<Mechanism>();
    }
}
