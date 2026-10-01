using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Properties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Data;
using System.Windows.Input;
namespace SensorMap.ViewModel
{
    public class DataBasePageVM : ReactiveObject, IActivatableViewModel
    {
        private string _dbName = string.Empty;
        private string _dbPath;
        private readonly IDataBaseProvider _dbProvider;
        private readonly IDataService _data;
        private ILogEntryService _logService;
        private string _logSearch = "";
        private bool _onlyErrors;
        private DbActionLogs? _selectedLog;

        public ObservableCollection<DbActionLogs> Logs => _logService.Logs;
        public bool CanLoadMore => _logService.CanLoadMore;

        public string DbName
        {
            get { return _dbName; }
            set { this.RaiseAndSetIfChanged(ref _dbName, value); }
        }
        public string DbPath
        {
            get { return _dbPath; }
            set { this.RaiseAndSetIfChanged(ref _dbPath, value); }
        }
        public ICollectionView LogsView { get; }

        public string LogSearch
        {
            get => _logSearch;
            set { this.RaiseAndSetIfChanged(ref _logSearch, value); LogsView.Refresh(); }
        }

        public bool OnlyErrors
        {
            get => _onlyErrors;
            set { this.RaiseAndSetIfChanged(ref _onlyErrors, value); LogsView.Refresh(); }
        }

        public DbActionLogs? SelectedLog
        {
            get => _selectedLog;
            set => this.RaiseAndSetIfChanged(ref _selectedLog, value);
        }
        public ViewModelActivator Activator { get; } = new ViewModelActivator();
        public DataBasePageVM(IDataBaseProvider dbProvider, IDataService data, ILogEntryService logService)
        {
            _dbProvider = dbProvider;
            _data = data;
            _logService = logService;
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
            LoadMoreLogs = new RelayCommand(() =>
            {
                _logService.LoadMore();
                this.RaisePropertyChanged(nameof(CanLoadMore));
            });
            LogsView = CollectionViewSource.GetDefaultView(Logs);
            LogsView.Filter = o =>
                o is DbActionLogs l
                && (!OnlyErrors || l.Kind == DbActionKind.Error)
                && (string.IsNullOrWhiteSpace(LogSearch)
                    || l.Description.Contains(LogSearch, StringComparison.CurrentCultureIgnoreCase));

            CopyLog = new RelayCommand(() =>
            {
                if (SelectedLog == null) return;
                var text = $"{SelectedLog.Timestamp:dd.MM.yyyy HH:mm:ss} [{SelectedLog.KindText}] {SelectedLog.Description}";
                if (!string.IsNullOrEmpty(SelectedLog.Technical))
                    text += Environment.NewLine + SelectedLog.Technical;
                System.Windows.Clipboard.SetText(text);
            });
            ShowFullLog = new RelayCommand(() =>
            {
                if (SelectedLog == null) return;
                string text = SelectedLog.Description;
                if(SelectedLog.Kind==DbActionKind.Error && !string.IsNullOrEmpty(SelectedLog.Technical))
                    text += Environment.NewLine + SelectedLog.Technical;
                HandyControl.Controls.MessageBox.Show(text, "Текст лога", System.Windows.MessageBoxButton.OK);
            });
        }
        public ICommand ChangeDataBase { get; private set; }
        public ICommand NavigateToPathDB { get; }
        public ICommand LoadMoreLogs { get; }
        public ICommand CopyLog { get; }
        public ICommand ShowFullLog { get; }
    }
}
