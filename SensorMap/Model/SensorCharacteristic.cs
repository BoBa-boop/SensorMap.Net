using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Model
{
    public class SensorCharacteristic:ReactiveObject
    {
        [Key]
        public int Id { get; set; }
        [Reactive] public string Title { get; set; } = string.Empty;
        [Reactive] public int SensorTypeId { get; set; }
        [Reactive] public virtual SensorType SensorType { get; set; } = null!;

    }
}
