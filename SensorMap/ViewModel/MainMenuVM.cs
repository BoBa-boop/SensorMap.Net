using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using SensorMap.Interfaces;
using SensorMap.Model;
using ReactiveUI.SourceGenerators;
using System.IO;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using SensorMap.Services.UpdateService;
using System.Windows.Input;
using SensorMap.View;

namespace SensorMap.ViewModel
{
    public class MainMenuVM:ReactiveObject, IActivatableViewModel
    {
        private IAuthorization _auth;
        private IJsonSerialization _json;
        private SettingsApp settings;
        
        private string DefaultImage = "\\Resources\\MenuImages\\menuImage2.jpg";
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
            this.WhenActivated((CompositeDisposable disposables) => {
                
                if (Path.Exists(SettingsApp.SettingsPath)) MySettings = _json.ReadFromJsonFile<SettingsApp>(SettingsApp.SettingsPath);
                else
                {
                    MySettings = new SettingsApp();
                    MySettings.MenuImages = new(new[] { "", "", "" });
                    _json.WriteToJsonFile<SettingsApp>(SettingsApp.SettingsPath, MySettings);
                }
                var listImages = MySettings.MenuImages.Where(x => !string.IsNullOrEmpty(x)).ToList();
                if (listImages.Count == 0) listImages.Add(DefaultImage);
                MenuImages = new(listImages);
            });
        }

        
    }
}
