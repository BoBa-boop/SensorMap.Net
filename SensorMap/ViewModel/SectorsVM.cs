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

namespace SensorMap.ViewModel
{
    public class SectorsVM:ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        private INavigation navigation; 
        private ObservableCollection<Sector> _coll;

        [Reactive]
        public ObservableCollection<Sector> Sectors
        {
            get => _coll;
            set => this.RaiseAndSetIfChanged(ref _coll, value);
        }
        [Reactive] public Sector? SelectedSector { get; set; }
        public SectorsVM(INavigation _nav,AppDBContext appDB)
        {
            navigation = _nav;
            GoToSector = new RelayCommand<object>((s) => navigation.ShowDialog<MechanismView,MechanismVM>());
            BackMenu = new RelayCommand(() => navigation.NavigateTo<MenuButtonsVM>());
            _provider = provider;

            Sectors = new ObservableCollection<Sector>();
        }

        

        public ICommand GoToSector { get; set; }
        public ICommand BackMenu { get; set; }
    }
}
