using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using System.Collections.ObjectModel;

namespace SensorMap.ViewModel
{
    public class PLC_VM:ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        private readonly IDataService _service;
        private PLC _plcSelected;
        [Reactive]
        public PLC SelectedPLC
        {
            get => _plcSelected;
            set { this.RaiseAndSetIfChanged(ref _plcSelected, value); }
        }
        [Reactive] public ObservableCollection<PLC> PLC { get; set; }
        private ObservableCollection<string> _manufacturers { get; set; }
        [Reactive] public TreeViewCollection<string, PLC> PLCTree { get; set; }
        public PLC_VM(IDataBaseProvider provider, IDataService service, PLC plc = null)
        {
            _service = service;
            _provider = provider;
            SelectedPLC = plc;            
            PLC = service.PLCs;
            _manufacturers = service.PLC_Manufacturers;
            Func<string, PLC, bool> filter = (m, p) => p.Manufacturer == m;
            PLCTree = new TreeViewCollection<string, PLC>("Manufacturer", _manufacturers, PLC, filter);
        }

    }
}
