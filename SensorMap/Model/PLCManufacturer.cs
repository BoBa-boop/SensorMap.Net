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
    public class PLCManufacturer : ReactiveObject
    {
        private string _name;
        private bool _isNew;

        [Key]
        public int Id { get; set; }
        [Reactive]
        [MaxLength(155)]
        public string Name
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

        [NotMapped]
        [Reactive]
        public bool IsNew
        {
            get => _isNew;
            set
            {
                this.RaiseAndSetIfChanged(ref _isNew, value);
            }
        }
        [Reactive] public ObservableCollection<PLC>? PLCs { get; set; }
    }
}
