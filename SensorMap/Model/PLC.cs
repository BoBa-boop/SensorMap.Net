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
        public Guid Id {  get; set; }
        public string TypePLC { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;

        public Guid InputsId { get; set; }
        public ObservableCollection<PLCInputs>? Inputs { get; set; }
        public Guid MechId { get; set; }
        public Mechanism? Mechanism { get; set; }
    }
}
