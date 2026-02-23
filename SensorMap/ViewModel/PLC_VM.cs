using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.EF;
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
        private IAppDbContextFactory _appDbContextFactory;
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
        public PLC_VM(IDataBaseProvider provider, IDataService service,IAppDbContextFactory appDbContextFactory, PLC plc = null)
        {
            _service = service;
            _provider = provider;
            _appDbContextFactory = appDbContextFactory;
            using (var _dbContext = _appDbContextFactory.CreateDbContext())
            {
                PLC = new(_dbContext.PLCs.ToList());
                _manufacturers = new (PLC.Select(plc => plc.Manufacturer).Distinct().ToList());
            }
            SelectedPLC = plc;            
            
            
            Func<string, PLC, bool> filter = (m, p) => p.Manufacturer == m;
            PLCTree = new TreeViewCollection<string, PLC>("Manufacturer", _manufacturers, PLC, filter);
        }

    }
}
