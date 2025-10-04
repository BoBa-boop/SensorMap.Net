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

namespace SensorMap.ViewModel
{
    public class SectorsVM:ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        private INavigation navigation;
        [Reactive] public ObservableCollection<Sector> Sectors { get; set; }
        [Reactive] public Sector? SelectedSector { get; set; }
        public SectorsVM(INavigation _nav, IDataBaseProvider provider)
        {
            navigation = _nav;
            GoToSector = new RelayCommand<object>((s) => navigation.ShowDialog<MechanismView>());
            BackMenu = new RelayCommand(() => navigation.NavigateTo<MenuButtonsVM>());
            _provider = provider;

            Sectors = new ObservableCollection<Sector>(await _provider.GetAllSectorsAsync());
        }

        public ICommand GoToSector { get; set; }
        public ICommand BackMenu { get; set; }
    }
}
