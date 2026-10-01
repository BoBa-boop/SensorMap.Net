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

namespace SensorMap.ViewModel
{
    public class SettingsVM:ReactiveObject
    {
        private IAuthorization _auth;
        private IDataBaseProvider _dbProvider;
        private IDataService _data;
        public SettingsVM(IAuthorization authorization, IDataService data, IDataBaseProvider dbProvider, ILogEntryService logService)
        {
            _dbProvider = dbProvider;
            _data = data;
            _auth = authorization;
            
            ChangeEditorPassword = new RelayCommand<string>((newPass) => _auth.ChangePassword(newPass), (newPass) => !string.IsNullOrEmpty(newPass));
        }

        public ICommand ChangeEditorPassword { get;private set; }
        
        
    }
}
