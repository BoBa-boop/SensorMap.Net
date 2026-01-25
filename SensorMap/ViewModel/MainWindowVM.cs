using CommunityToolkit.Mvvm.Input;
using DynamicData.Binding;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Services;
using SensorMap.View;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class MainWindowVM:ReactiveObject
    {
        private readonly IDataService _dataService;
        [Reactive] public INavigation Navigation { get; set; }

        private bool _isEdit = false;
        private bool _activeWindow=true;

        [Reactive]
        public bool IsEditMode
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
            IsEditMode = _dataService.IsEditMode;
            NavigateToSectors = new RelayCommand(() => Navigation.NavigateTo<SectorsVM>());
            NavigateToSettings = new RelayCommand(() => Navigation.NavigateTo<SettingsVM>());
            NavigateToSensors = new RelayCommand(() => Navigation.NavigateTo<SensorVM>());
            NavigateToDB = new RelayCommand(() => Navigation.NavigateTo<CRUD_VM>());
            NavigateToMenu = new RelayCommand(() => Navigation.NavigateTo<MainMenuVM>());
            NavigateToMechanisms = new RelayCommand(() => Navigation.NavigateTo<MechanismVM>());
            NavigateToPLC = new RelayCommand(()=>Navigation.NavigateTo<PLC_VM>());
            TurnOnEditMode = new RelayCommand(() => OpenAuthWindow());

            this.WhenAnyValue(x => x.IsEditMode)
                .BindTo(_dataService, x => x.IsEditMode);

            _dataService.WhenAnyValue(x => x.IsEditMode)
                .BindTo(this, x => x.IsEditMode);
        }

        private void OpenAuthWindow()
        {
            ActiveWindow = false;
            Navigation.ShowDialog<AuthorizationWindow, AuthVM>();
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
