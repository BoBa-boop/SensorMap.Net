using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class SettingsVM:ReactiveObject
    {
        private IAuthorization _auth;
        private string _dbName = string.Empty;

        [Reactive]public string DbName
        {
            get { return _dbName; }
            set { this.RaiseAndSetIfChanged(ref _dbName, value); }
        }

        public SettingsVM(IAuthorization authorization)
        {
            _auth = authorization;
            DbName = Properties.Settings.Default.ConnectionString;
            ChangeEditorPassword = new RelayCommand<string>((newPass) => _auth.ChangePassword(newPass),(newPass)=>!string.IsNullOrEmpty(newPass));
        }

        public ICommand ChangeEditorPassword { get;private set; }
    }
}
