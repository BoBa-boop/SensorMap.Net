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
        public int Id { get; set; }
        public int SensorId { get; set; }
        public Sensor? Sensor { get; set; }
        public int PLCId { get; set; }
        public PLC? PLC { get; set; }

        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
