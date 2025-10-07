using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.View;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class MenuButtonsVM:ReactiveObject
    {
        private readonly IDataService _dataService;
        [Reactive] public INavigation? Navigation { get; set; }

        private bool _isEdit;

        [Reactive]
        public bool IsEdit
        {
            get => _isEdit;
            set => this.RaiseAndSetIfChanged(ref _isEdit, value);
        }
        public MenuButtonsVM(INavigation _nav,IDataService service)
        {
            _dataService = service;
            Navigation = _nav;
            NavigateToSectors = new RelayCommand(() => Navigation.NavigateTo<SectorsVM>());
            NavigateToSettings = new RelayCommand(() => Navigation.NavigateTo<SettingsVM>());
            NavigateToSensors = new RelayCommand(() => Navigation.ShowDialog<SensorView, SensorVM>());
            NavigateToCRUD = new RelayCommand(() => Navigation.ShowDialog<CRUD_View, CRUD_VM>());


            this.WhenAnyValue(x => x._dataService.IsEditMode).Subscribe((value) => IsEdit = value);

        }


        public ICommand NavigateToCRUD{ get; set; }
        public ICommand NavigateToSectors { get; set; }
        public ICommand NavigateToSensors { get; set; }
        public ICommand NavigateToSettings { get; set; }
        public ICommand NavigateToMenu { get; set; }
    }
}
