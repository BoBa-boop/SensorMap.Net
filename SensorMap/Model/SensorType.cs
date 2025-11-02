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
    public class SensorType:ReactiveObject
    {
        private string _name;
        [Key]
        [Reactive] public int Id { get; set; }
        [MaxLength(30)]
        [Reactive]public string Name
        {
            get => _name;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.RaiseAndSetIfChanged(ref _name, value);
                }
            } 
        }
    }
}
