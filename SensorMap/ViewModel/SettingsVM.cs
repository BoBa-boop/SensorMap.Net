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
        private IAppDbContextFactory _appDbContextFactory;
        private AppDBContext _dbContext;
        private IDataBaseProvider _dbProvider;
        private string _dbName = string.Empty;

        [Reactive]public string DbName
        {
            get { return _dbName; }
            set { this.RaiseAndSetIfChanged(ref _dbName, value); }
        }

        public SettingsVM(IAuthorization authorization, IAppDbContextFactory appDbContextFactory, IDataBaseProvider dbProvider)
        {
            _dbProvider = dbProvider;
            _appDbContextFactory = appDbContextFactory;
            _dbContext = appDbContextFactory.CreateDbContext();
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
                    throw new Exception("Неправильная работа бэкап. Не отключает прошлое соединение и уничтожает данные");
                    //_dbProvider.ChangeDataBase(fileBrowser.FileName);
                    //DbName = Path.GetFileName(Settings.Default.ConnectionString);
                }

            });
        }

        public ICommand ChangeEditorPassword { get;private set; }
        public ICommand ChangeDataBase { get; private set; }
    }
}
