using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Services;
using SensorMap.View;
using System.Windows;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class MainWindowVM:ReactiveObject
    {
        private readonly IDataService _dataService;
        private readonly IConfiguration _configuration;
        [Reactive] public INavigation? Navigation { get; set; }

        private bool _isEdit = false;
        private bool _activeWindow=true;

        [Reactive]
        public bool IsEdit
        {
            get => _isEdit;
            set => this.RaiseAndSetIfChanged(ref _isEdit, value);
        }
        [Reactive]
        public bool ActiveWindow
        {
            get => _activeWindow;
            set => this.RaiseAndSetIfChanged(ref _activeWindow, value);
        }
        public MainWindowVM(INavigation _nav, IDataService service)
        {
            _dataService = service;
            Navigation = _nav;
            NavigateToSectors = new RelayCommand(() => Navigation.NavigateTo<SectorsVM>());
            NavigateToSettings = new RelayCommand(() => Navigation.NavigateTo<SettingsVM>());
            NavigateToSensors = new RelayCommand(() => Navigation.NavigateTo<SensorVM>());
            NavigateToDB = new RelayCommand(() => Navigation.NavigateTo<CRUD_VM>(false));
            NavigateToMenu = new RelayCommand(() => Navigation.NavigateTo<MainMenuVM>());
            NavigateToMechanisms = new RelayCommand(() => Navigation.NavigateTo<MechanismVM>());
            NavigateToPLC = new RelayCommand(()=>Navigation.NavigateTo<PLC_VM>());
            this.WhenAnyValue(x => x._dataService.IsEditMode).Subscribe((value) => IsEdit = value);
            TurnOnEditMode = new RelayCommand(() => OpenAuthWindow());
        }

        private void OpenAuthWindow()
        {
            ActiveWindow = false;
            if(!IsEdit) Navigation.ShowDialog<AuthorizationWindow, AuthVM>();
            if(IsEdit)Navigation.NavigateTo<CRUD_VM>(IsEdit);
            ActiveWindow = true; 
        }

        public ICommand TurnOnEditMode { get; }
        public ICommand NavigateToDB { get; set; }
        public ICommand NavigateToSectors { get; set; }
        public ICommand NavigateToSensors { get; set; }
        public ICommand NavigateToSettings { get; set; }
        public ICommand NavigateToMenu { get; set; }
        public ICommand NavigateToPLC { get; set; }
        public ICommand NavigateToMechanisms { get; set; }
    }
}
