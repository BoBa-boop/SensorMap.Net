using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace SensorMap.ViewModel
{
    public class SettingsVM:ReactiveObject
    {
        private IAuthorization _auth;
        private IJsonSerialization _json;
        private SettingsApp settings;

        [Reactive]public SettingsApp MySettings { get => settings; set => settings = value; }
        public SettingsVM(IAuthorization authorization, IJsonSerialization jsonSerialization)
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

            CreateRecoveryCodes = new RelayCommand(() => _auth.GenerateRecoveryCodes());
            SaveMenuImages = new RelayCommand(() => _json.WriteToJsonFile("SettingsApp.json", MySettings));
        }

        public ICommand CreateRecoveryCodes { get;private set; }
        public ICommand SaveMenuImages { get; }
    }
}
