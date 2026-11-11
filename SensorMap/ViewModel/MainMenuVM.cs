using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services.UpdateService;
using SensorMap.View;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Disposables;
using System.Windows.Input;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SensorMap.ViewModel
{
    public class MainMenuVM:ReactiveObject, IActivatableViewModel
    {
        private readonly Updater _updater = new();
        private bool isChecked = false;
        private IAuthorization _auth;
        private IJsonSerialization _json;
        private SettingsApp settings;
        [Reactive] public SettingsApp MySettings { get => settings; set => settings = value; }

        private ObservableCollection<string> _images;

        public ObservableCollection<string> MenuImages
        {
            get { return _images; }
            set => this.RaiseAndSetIfChanged(ref _images, value);
        }
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
        public MainMenuVM(IAuthorization authorization, IJsonSerialization jsonSerialization)
        {
            _auth = authorization;
            _json = jsonSerialization;
            Task.Run(async () => {
                if (!isChecked)
                {
                    isChecked = await _updater.CheckAsync();
                    if (isChecked)
                    {
                        Growl.Info("Доступно обновление!");
                    }
                }
            });
            
            this.WhenActivated((CompositeDisposable disposables) => {
                
                if (Path.Exists(SettingsApp.SettingsPath)) MySettings = _json.ReadFromJsonFile<SettingsApp>(SettingsApp.SettingsPath);
                else
                {
                    MySettings = new SettingsApp();
                    MySettings.MenuImages = new(new[] { "", "", "" });
                    _json.WriteToJsonFile<SettingsApp>(SettingsApp.SettingsPath, MySettings);
                }
                var listImages = MySettings.MenuImages.Where(x => !string.IsNullOrEmpty(x)).ToList();
                MenuImages = new(listImages);
            });
        }

        
    }
}
