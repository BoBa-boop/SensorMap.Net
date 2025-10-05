using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.View;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class MenuButtonsVM:ReactiveObject
    {
        [Reactive] public INavigation? Navigation { get; set; }
        public MenuButtonsVM(INavigation _nav) 
        {
            Navigation = _nav; 
            NavigateToSectors = new RelayCommand(() => Navigation.NavigateTo<SectorsVM>());
            NavigateToSettings = new RelayCommand(() => Navigation.NavigateTo<SettingsVM>());
            NavigateToSensors = new RelayCommand(() => Navigation.ShowDialog<SensorView,SensorVM>());
        }
        public ICommand NavigateToSectors { get; set; }
        public ICommand NavigateToSensors { get; set; }
        public ICommand NavigateToSettings { get; set; }
        public ICommand NavigateToMenu { get; set; }
    }
}
