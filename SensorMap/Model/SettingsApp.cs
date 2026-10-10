using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Model
{
    public class SettingsApp:ReactiveObject
    {
        private static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), // %APPDATA%\Roaming
                "SensorMap"
);

        public static readonly string SettingsPath = Path.Combine(SettingsDir, "SettingsApp.json");
        private ObservableCollection<string> menuImg = new();

		[Reactive]
		[JsonProperty("Картинки меню")]
		public ObservableCollection<string> MenuImages
		{
			get { return menuImg; }
			set { menuImg = value; }
		}

		
	}
}
