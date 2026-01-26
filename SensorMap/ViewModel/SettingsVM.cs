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
        private IDataService _dataService;
        private string _dbName = string.Empty;

        [Reactive]public string DbName
        {
            get { return _dbName; }
            set { this.RaiseAndSetIfChanged(ref _dbName, value); }
        }

        public SettingsVM(IAuthorization authorization, IDataService dataService)
        {
            _dataService = dataService;
            _auth = authorization;
            DbName = _dataService.GetConnectionString();
            ChangeEditorPassword = new RelayCommand<string>((newPass) => _auth.ChangePassword(newPass), (newPass) => !string.IsNullOrEmpty(newPass));
            
            //ChangeDataBase = new RelayCommand
        }

        public ICommand ChangeEditorPassword { get;private set; }
        public ICommand ChangeDataBase { get; private set; }
    }
}
