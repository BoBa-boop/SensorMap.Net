using CommunityToolkit.Mvvm.Input;
using DynamicData.Binding;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Services;
using SensorMap.View;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class MainWindowVM:ReactiveObject,IActivatableViewModel
    {
        private readonly IDataBaseProvider _provider;
        private readonly IDataService _dataService;
        [Reactive] public INavigation Navigation { get; set; }
        private bool _activeWindow=true;

        private readonly ObservableAsPropertyHelper<bool> _isConnectedDbHelper; 
        public bool IsConnectedDB => _isConnectedDbHelper.Value;

        private readonly ObservableAsPropertyHelper<bool> _isEditMode;
        public bool IsEditMode => _isEditMode.Value;
        public bool ActiveWindow
        {
            get => _activeWindow;
            set => this.RaiseAndSetIfChanged(ref _activeWindow, value);
        }
        public ViewModelActivator Activator { get; }
        public MainWindowVM(INavigation _nav, IDataService service, IDataBaseProvider provider)
        {
            Activator = new ViewModelActivator();
            _provider = provider;
            _dataService = service;
            Navigation = _nav;
            _isEditMode = _dataService.WhenAnyValue(x => x.IsEditMode)
                .ToProperty(this, x => x.IsEditMode);
#if DEBUG==true
            _dataService.IsEditMode = true;
#endif
            
            NavigateToSectors = new RelayCommand(() => Navigation.NavigateTo<SectorsVM>());
            NavigateToSettings = new RelayCommand(() => Navigation.NavigateTo<SettingsVM>());
            NavigateToSensors = new RelayCommand(() => Navigation.NavigateTo<SensorVM>());
            NavigateToDB = new RelayCommand(() => Navigation.NavigateTo<CRUD_VM>());
            NavigateToMenu = new RelayCommand(() => Navigation.NavigateTo<MainMenuVM>());
            NavigateToMechanisms = new RelayCommand(() => Navigation.NavigateTo<MechanismVM>());
            NavigateToDevices = new RelayCommand(()=>Navigation.NavigateTo<Devices_VM>());
            TurnOnEditMode = new RelayCommand(() => OpenAuthWindow());
            SetViewMode = new RelayCommand(() => _dataService.IsEditMode = false);
            CreateBackupDB = new RelayCommand(() => 
            {
                FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
                folderBrowser.Multiselect = false;
                folderBrowser.ShowDialog();
                if(!string.IsNullOrEmpty(folderBrowser.SelectedPath))
                    _provider.CreateBackupDB(folderBrowser.SelectedPath);
            });
            _isConnectedDbHelper = _dataService.WhenAnyValue(x => x.IsDataBaseConnect)
                .ToProperty(this, x => x.IsConnectedDB);
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.IsEditMode).Subscribe((mode) =>
                {
                    if (mode == false) NavigateToMenu.Execute(null);
                }).DisposeWith(disposables);
            });
        }

        private void OpenAuthWindow()
        {
            ActiveWindow = false;
            Navigation.ShowDialog<AuthorizationWindow, AuthVM>();
            ActiveWindow = true; 
        }

        public ICommand TurnOnEditMode { get; }
        public ICommand SetViewMode { get; }
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
