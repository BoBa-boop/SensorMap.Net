using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Key]
        public Guid Id {  get; set; }
        public Guid SensorID { get; set; }
        public Sensor? Sensor { get; set; }
        public Guid PLCId { get; set; }
        public PLC? PLC { get; set; }
        public Guid MechID { get; set; }
        public Mechanism? Mechanism { get; set; }

        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty; 
        public double XPoint { get; set; }
        public double YPoint { get; set; }
    }
}
