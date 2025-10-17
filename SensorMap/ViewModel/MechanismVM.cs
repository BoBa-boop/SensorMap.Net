using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    /// <summary>
    /// ПУСТОЙ Mechanisms у секторов
    /// </summary>
    public class MechanismVM:ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        private readonly IDataService _service;
        [Reactive]public Sector? CurrentSector { get; set; }
        [Reactive] public ObservableCollection<Sector> Sectors { get; set; } = new();
        [Reactive] public ObservableCollection<Sensor> Sensors { get; set; } = new();
        [Reactive] public ObservableCollection<Mechanism> Mechanisms { get; set; } = new();
        public MechanismVM(IDataBaseProvider provider, IDataService service,Sector sector=null)
        {            
            _provider = provider;
            _service = service;
            CurrentSector = sector;
            Sectors = _service.Sectors;
            Sensors = _service.Sensors;
            Mechanisms = new (_service.Mechanisms.Where(x => x.Sector != null && x.Sector.Id == (CurrentSector?.Id ?? 0)).ToList());
        }
        public ICommand SaveLayout {  get; set; }
    }
}
