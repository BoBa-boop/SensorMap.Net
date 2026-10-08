using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using SensorMap.Interfaces;
using SensorMap.Model;
using ReactiveUI.SourceGenerators;
using System.IO;

namespace SensorMap.ViewModel
{
    public class MainMenuVM:ReactiveObject
    {
        private IAuthorization _auth;
        private IJsonSerialization _json;
        private SettingsApp settings;

        [Reactive] public SettingsApp MySettings { get => settings; set => settings = value; }
        public MainMenuVM(IAuthorization authorization, IJsonSerialization jsonSerialization)
        {
            _auth = authorization;
            _json = jsonSerialization;

            if (Path.Exists("SettingsApp.json")) MySettings = _json.ReadFromJsonFile<SettingsApp>("SettingsApp.json");
            else
            {
                MySettings = new SettingsApp();
                MySettings.MenuImages = new(new[] { "", "", "" });
                _json.WriteToJsonFile<SettingsApp>("SettingsApp.json", MySettings);
            }
        }
    }
}
