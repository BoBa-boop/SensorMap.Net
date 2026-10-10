using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Model
{
    public class SettingsApp:ReactiveObject
    {
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
