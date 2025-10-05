using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
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
        [Key]
        public int Id {  get; set; }
        public string Name {  get; set; } = string.Empty;
        public ObservableCollection<Mechanism>? Mechanisms { get; set; }
    }
}
