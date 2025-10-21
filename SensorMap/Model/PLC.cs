using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public int Id { get; set; }
        public string TypePLC { get; set; } = string.Empty;
        
        public byte[]? Image { get; set; }
        [MaxLength(15)]
        public string IP { get; set; } = string.Empty;

        //public Guid InputsId { get; set; }
        public ObservableCollection<PLCInputs>? Inputs { get; set; }
        public int MechId { get; set; }
        public Mechanism? Mechanism { get; set; }
    }
}
