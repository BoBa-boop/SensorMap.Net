using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.Logging;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.EF;
using SensorMap.Interfaces;
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
        private string _dbName = string.Empty;
        private string _dbPath;
        private ObservableCollection<DbActionLogs> _logs;

        [Reactive]public string DbName
        {
            get { return _dbName; }
            set { this.RaiseAndSetIfChanged(ref _dbName, value); }
        }
        [Reactive]public string DbPath
        {
            get { return _dbPath; }
            set { this.RaiseAndSetIfChanged(ref _dbPath, value); }
        }
        [Reactive] public ObservableCollection<DbActionLogs> Logs
        {
            get { return _logs; }
            set { this.RaiseAndSetIfChanged(ref _logs, value); }
        }
        public SettingsVM(IAuthorization authorization, IDataService data, IDataBaseProvider dbProvider)
        {
            _dbProvider = dbProvider;
            _data = data;
            _auth = authorization;
            //Logs = ReadLogFile("LogDb.txt");
            DbName = Path.GetFileName(Settings.Default.ConnectionString);
            DbPath = Path.GetFullPath(Settings.Default.ConnectionString.Replace("DataSource=", ""));
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
                        DbPath = fileBrowser.FileName;
                        DbName = Path.GetFileName(Settings.Default.ConnectionString);
                        Growl.Success("Выбрана новая База Данных");
                        _data.IsDataBaseConnect = true;
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message, "Ошибка при изменение БД", System.Windows.MessageBoxButton.OK);
                        _data.IsDataBaseConnect = false;
                    }
                }

            });
            NavigateToPathDB = new RelayCommand(() =>
            {
                if (Directory.Exists(Path.GetDirectoryName(DbPath)))
                {
                    Process.Start("explorer.exe", Path.GetDirectoryName(DbPath));
                }
            });
        }

        //private static ObservableCollection<DbActionLogs> ReadLogFile(string path)
        //{
        //    return new(File.ReadAllText(path));
        //}

        public ICommand ChangeEditorPassword { get;private set; }
        public ICommand ChangeDataBase { get; private set; }
        public ICommand NavigateToPathDB { get; }
        
    }
}
