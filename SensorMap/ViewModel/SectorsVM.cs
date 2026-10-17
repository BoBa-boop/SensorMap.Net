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
using System.Reactive.Disposables;

namespace SensorMap.ViewModel
{
    public class SectorsVM: ReactiveObject, IActivatableViewModel
    {
        private INavigation navigation;
        private IAppDbContextFactory _appDbContextFactory;
        private ObservableCollection<Sector>? _coll;
        private string _searchText = string.Empty;
        private Sector? _sect;
        private List<Sector> _allSectors = new();
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
        [Reactive] public Sector? SelectedSector 
        {
            get => _sect;
            set => this.RaiseAndSetIfChanged(ref _sect, value);
        }
        [Reactive]
        public bool IsDetailOpen
        {
            get => _isDetailOpen;
            set => this.RaiseAndSetIfChanged(ref _isDetailOpen, value);
        }
        private bool _isDetailOpen;
        public ICommand ClosePanelCommand { get; }
        public SectorsVM(INavigation _nav,  IAppDbContextFactory cxFactory)
        {            
            navigation = _nav;
            _appDbContextFactory = cxFactory;
            GoToMech = new RelayCommand<Mechanism>((mech) => { if (mech != null) navigation.NavigateTo<MechanismVM>(mech); },(mech) => { return mech != null; });
            ClosePanelCommand = new RelayCommand(() => SelectedSector = null);
            using (var _dbContext = _appDbContextFactory.CreateDbContext())
            {
                _allSectors = new(_dbContext.Sectors.Include(x => x.Mechanisms).AsNoTracking().ToList());
                Sectors = new(_allSectors);
            }
            this.WhenActivated(disposables => {
                this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        var result = from sector in _allSectors
                                     where sector.Name.ToLower().Contains(SearchText.ToLower()) ||
                                     sector.Mechanisms != null && sector.Mechanisms.Any(m => m.Name.ToLower().Contains(SearchText))
                                     select sector;
                        Sectors = new(result);
                    }
                    else Sectors = new(_allSectors);
                })
                .DisposeWith(disposables);
                this.WhenAnyValue(x => x.SelectedSector)
                    .Subscribe(sector => IsDetailOpen = sector != null)
                    .DisposeWith(disposables);
            });
        }
        public ICommand GoToMech { get; set; }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();
    }
}
