using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Model;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class MechanismVM:ReactiveObject
    {
        [Reactive]public Sector CurrentSector { get; set; } 
        public MechanismVM()
        {
           
        }
        public ICommand SaveLayout {  get; set; }
    }
}
