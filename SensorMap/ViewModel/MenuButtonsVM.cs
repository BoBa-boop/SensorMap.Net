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
        [Reactive] public INavigation Navigation { get; set; }
        public MenuButtonsVM(INavigation _nav) 
        {
            Navigation = _nav; 
            NavigateToSectors = new RelayCommand(() => Navigation.NavigateTo<SectorsVM>());
            NavigateToSensors = new RelayCommand(() => Navigation.ShowDialog<SensorView>());
        }
        public ICommand NavigateToSectors { get; set; }
        public ICommand NavigateToSensors { get; set; }
        public ICommand NavigateToMenu { get; set; }
    }
}
