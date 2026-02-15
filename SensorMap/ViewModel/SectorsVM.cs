using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SensorMap.Interfaces;
using CommunityToolkit.Mvvm.Input;
using SensorMap.View;
using SensorMap.EF;
using Microsoft.EntityFrameworkCore;
using System.Windows.Documents;
using DynamicData;
using System.Reactive.Linq;
using System.Windows;

namespace SensorMap.ViewModel
{
    public class SectorsVM:ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        private readonly IDataService _data;
        private INavigation navigation;
        private IAppDbContextFactory _appDbContextFactory;
        private AppDBContext _dbContext;
        private ObservableCollection<Sector>? _coll;
        private string _searchText = string.Empty;


        [Reactive]
        public string SearchText
        {
            get =>_searchText; 
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }


        [Reactive]
        public ObservableCollection<Sector>? Sectors
        {
            get => _coll;
            set => this.RaiseAndSetIfChanged(ref _coll, value);
        }
        [Reactive] public Sector? SelectedSector { get; set; }
        public SectorsVM(INavigation _nav, IDataBaseProvider provider, IAppDbContextFactory cxFactory, IDataService data)
        {            
            navigation = _nav;
            _data = data;
            _appDbContextFactory = cxFactory;
            _dbContext = _appDbContextFactory.CreateDbContext();
            GoToSector = new RelayCommand<Sector>((sector) => 
            {
                if(sector != null) 
                {
                    _data.CurrentSector_Global = sector;
                    navigation.NavigateTo<MechanismVM>();
                } 
            });
            _provider = provider;

            Sectors = new(_dbContext.Sectors.ToList());
            var tempCollection = Sectors;
            this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var result = from sector in tempCollection
                                 where sector.Name.ToLower().Contains(SearchText) ||
                                 sector.Mechanisms != null && sector.Mechanisms.Any(m => m.Name.ToLower().Contains(SearchText))
                                 select sector;
                    Sectors = new(result);
                }
                else Sectors = new ObservableCollection<Sector>(_dbContext.Sectors.ToList());
            });

        }
        public ICommand GoToSector { get; set; }
    }
}
