using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.View;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class MainWindowVM:ReactiveObject
    {
        private readonly IDataService _dataService;
        [Reactive] public INavigation? Navigation { get; set; }

        private bool _isEdit = false;

        [Reactive]
        public bool IsEdit
        {
            get => _isEdit;
            set => this.RaiseAndSetIfChanged(ref _isEdit, value);
        }
        public MainWindowVM(INavigation _nav, IDataService service)
        {
            _dataService = service;
            Navigation = _nav;
            NavigateToSectors = new RelayCommand(() => Navigation.NavigateTo<SectorsVM>());
            NavigateToSettings = new RelayCommand(() => Navigation.NavigateTo<SettingsVM>());
            NavigateToSensors = new RelayCommand(() => Navigation.NavigateTo<SensorVM>());
            NavigateToDB = new RelayCommand(() => Navigation.NavigateTo<CRUD_VM>(IsEdit));
            NavigateToMenu = new RelayCommand(() => Navigation.NavigateTo<MainMenuVM>());
            NavigateToMechanisms = new RelayCommand(() => Navigation.NavigateTo<MechanismVM>());

            this.WhenAnyValue(x => x._dataService.IsEditMode).Subscribe((value) => IsEdit = value);
            TurnOnEditMode = new RelayCommand(() => OpenAuthWindow());
        }

        private void OpenAuthWindow()
        {
            AuthorizationWindow authorizationWindow = new AuthorizationWindow();
            authorizationWindow.Show();
        }

        public ICommand TurnOnEditMode { get; }
        public ICommand NavigateToDB { get; set; }
        public ICommand NavigateToSectors { get; set; }
        public ICommand NavigateToSensors { get; set; }
        public ICommand NavigateToSettings { get; set; }
        public ICommand NavigateToMenu { get; set; }
        public ICommand NavigateToMechanisms { get; set; }
    }
}
