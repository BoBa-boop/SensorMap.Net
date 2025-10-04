using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.EF;
using SensorMap.Interfaces;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class MenuVM:ReactiveObject
    {
        private IDataService _dataService;
        [Reactive] public INavigation Navigation { get; set; }
        [Reactive] public bool IsEditMode {  get; set; }
        public MenuVM(IDataService dataService, INavigation _nav)
        {
            Navigation = _nav;
            _dataService = dataService;
            ShowMenu = new RelayCommand(() => Navigation.NavigateTo<MenuButtonsVM>());
        }


        public ICommand ShowMenu { get; set; }
    }
}
