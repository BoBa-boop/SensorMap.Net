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
        private readonly IDataBaseProvider _provider;
        private readonly IDataService _dataService;
        [Reactive] public INavigation Navigation { get; set; }
        private bool _isEdit = false;
        private bool _activeWindow=true;
        private bool _isConnected = false;

        [Reactive]
        public bool IsEditMode
        {
            get => _isEdit;
            set => this.RaiseAndSetIfChanged(ref _isEdit, value);
        }
        [Reactive]
        public bool IsConnectedDB
        {
            get => _isConnected;
            set => this.RaiseAndSetIfChanged(ref _isConnected, value);
        }
        [Reactive]
        public bool ActiveWindow
        {
            get => _activeWindow;
            set => this.RaiseAndSetIfChanged(ref _activeWindow, value);
        }
        public MainWindowVM(INavigation _nav, IDataService service, IDataBaseProvider provider)
        {
            _provider = provider;
            _dataService = service;
            Navigation = _nav;
            IsEditMode = _dataService.IsEditMode;
#if DEBUG==true
            IsEditMode = true;
#endif
            
            NavigateToSectors = new RelayCommand(() => Navigation.NavigateTo<SectorsVM>());
            NavigateToSettings = new RelayCommand(() => Navigation.NavigateTo<SettingsVM>());
            NavigateToSensors = new RelayCommand(() => Navigation.NavigateTo<SensorVM>());
            NavigateToDB = new RelayCommand(() => Navigation.NavigateTo<CRUD_VM>());
            NavigateToMenu = new RelayCommand(() => Navigation.NavigateTo<MainMenuVM>());
            NavigateToMechanisms = new RelayCommand(() => Navigation.NavigateTo<MechanismVM>());
            NavigateToDevices = new RelayCommand(()=>Navigation.NavigateTo<Devices_VM>());
            TurnOnEditMode = new RelayCommand(() => OpenAuthWindow());
            CreateBackupDB = new RelayCommand(() => 
            {
                FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
                folderBrowser.Multiselect = false;
                folderBrowser.ShowDialog();
                if(!string.IsNullOrEmpty(folderBrowser.SelectedPath))
                    _provider.CreateBackupDB(folderBrowser.SelectedPath);
            });

            this.WhenAnyValue(x => x.IsEditMode)
                .BindTo(_dataService, x => x.IsEditMode);

            _dataService.WhenAnyValue(x => x.IsEditMode)
                .BindTo(this, x => x.IsEditMode);

            _dataService.WhenAnyValue(x => x.IsDataBaseConnect)
               .BindTo(this, x => x.IsConnectedDB);
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
        public ICommand NavigateToDevices { get; set; }
        public ICommand NavigateToMechanisms { get; set; }
        public ICommand CreateBackupDB { get; set; }
    }
}
