using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Properties;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
namespace SensorMap.ViewModel
{
    public class DataBasePageVM : ReactiveObject, IActivatableViewModel
    {
        private string _dbName = string.Empty;
        private string _dbPath;
        private readonly IDataBaseProvider _dbProvider;
        private readonly IDataService _data;

        [Reactive]
        public string DbName
        {
            get { return _dbName; }
            set { this.RaiseAndSetIfChanged(ref _dbName, value); }
        }
        [Reactive]
        public string DbPath
        {
            get { return _dbPath; }
            set { this.RaiseAndSetIfChanged(ref _dbPath, value); }
        }
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
        public DataBasePageVM(IDataBaseProvider dbProvider, IDataService data)
        {
            _dbProvider = dbProvider;
            _data = data;
            DbName = Path.GetFileName(Settings.Default.ConnectionString);
            DbPath = Path.GetFullPath(Settings.Default.ConnectionString.Replace("DataSource=", ""));
            ChangeDataBase = new RelayCommand(() =>
            {
                OpenFileDialog fileBrowser = new OpenFileDialog();
                fileBrowser.Multiselect = false;
                fileBrowser.Filter = "База Данных (*.db)|*.db|Sqlite (*.sqlite)|*.sqlite";
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
        public ICommand ChangeDataBase { get; private set; }
        public ICommand NavigateToPathDB { get; }
    }
}
