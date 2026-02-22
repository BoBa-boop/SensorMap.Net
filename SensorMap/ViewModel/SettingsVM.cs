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
        private IDataBaseProvider _dbProvider;
        private IDataService _data;
        private string _dbName = string.Empty;

        [Reactive]public string DbName
        {
            get { return _dbName; }
            set { this.RaiseAndSetIfChanged(ref _dbName, value); }
        }

        public SettingsVM(IAuthorization authorization, IDataService data, IDataBaseProvider dbProvider)
        {
            _dbProvider = dbProvider;
            _data = data;
            _auth = authorization;
            DbName = Path.GetFileName(Settings.Default.ConnectionString);
            ChangeEditorPassword = new RelayCommand<string>((newPass) => _auth.ChangePassword(newPass), (newPass) => !string.IsNullOrEmpty(newPass));

            ChangeDataBase = new RelayCommand(() =>
            {
                OpenFileDialog fileBrowser = new OpenFileDialog();
                fileBrowser.Multiselect = false;
                fileBrowser.ShowDialog();
                if (!string.IsNullOrEmpty(fileBrowser.FileName))
                {
                    try
                    {
                        _dbProvider.ChangeDataBase(fileBrowser.FileName);
                        DbName = Path.GetFileName(Settings.Default.ConnectionString);
                        Growl.Success("Выбрана новая База Данных");
                        _data.IsDataBaseConnect = true;
                    }
                    catch(Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message, "Ошибка при изменение БД", System.Windows.MessageBoxButton.OK);
                        _data.IsDataBaseConnect = false;
                    }
                }

            });
        }

        public ICommand ChangeEditorPassword { get;private set; }
        public ICommand ChangeDataBase { get; private set; }
        
    }
}
