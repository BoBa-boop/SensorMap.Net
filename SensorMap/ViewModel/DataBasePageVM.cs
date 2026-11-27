using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Data.Sqlite;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Properties;
using SensorMap.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using Path = System.IO.Path;
namespace SensorMap.ViewModel
{
    public class DataBasePageVM : ReactiveObject, IActivatableViewModel
    {
        private string _dbName = string.Empty;
        private string _dbPath = string.Empty;
        private readonly IDataBaseProvider _dbProvider;
        private readonly IDataService _data;
        private ILogEntryService _logService;
        private string _logSearch = "";
        private bool _onlyErrors;
        private string _userDbPath = string.Empty;
        private DbActionLogs? _selectedLog;

        public ObservableCollection<DbActionLogs> Logs => _logService.Logs; 
        public ObservableCollection<CheckStepVM> Steps { get; } = new();

        private int _stepIndex;
        public int StepIndex
        {
            get => _stepIndex;
            set => this.RaiseAndSetIfChanged(ref _stepIndex, value);
        }

        private string? _checkError;
        private bool _showUpdBtn;

        public string? CheckError
        {
            get => _checkError;
            set => this.RaiseAndSetIfChanged(ref _checkError, value);
        }
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
        public bool ShowUpdateDbButton
        {
            get => _showUpdBtn;
            set { this.RaiseAndSetIfChanged(ref _showUpdBtn, value); }
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
            DbPath = AppPaths.GetDbPath();
            DbName = AppPaths.GetDbName();
            
            ChangeDataBase = new RelayCommand(async() =>
            {
                OpenFileDialog fileBrowser = new OpenFileDialog();
                fileBrowser.Multiselect = false;
                fileBrowser.Filter = "База Данных (*.db)|*.db|Sqlite (*.sqlite)|*.sqlite";
                fileBrowser.ShowDialog();
                if (!string.IsNullOrEmpty(fileBrowser.FileName))
                {
                    _userDbPath = fileBrowser.FileName;
                    if(_userDbPath == DbPath)
                    {
                        Growl.Info("Выбранная БД уже загружена.");
                        return;
                    }
                    await TryConnectAsync(_userDbPath);
                }

            });
            UpdateDataBaseMigration = new RelayCommand(async () =>
            {
                if (string.IsNullOrEmpty(_userDbPath)) return;

                
                try
                {
                    var result = await _dbProvider.BackupAndMigrateAsync(_userDbPath);
                    Growl.Info(new GrowlInfo
                    {
                        Message = $"Создание резервной копии БД...",
                        StaysOpen = true,
                        ShowCloseButton = true,
                        ShowDateTime = false,
                        WaitTime = 5
                    });
                    if (!result.Item1.Ok)
                    {
                        Growl.Error(result.Item1.Message);
                        return;
                    }
                    await TryConnectAsync(_userDbPath);
                    ShowUpdateDbButton = false;
                    if (!File.Exists(result.Item2)) return;
                    var answer2 = HandyControl.Controls.MessageBox.Show(
                       "Удалить ранее созданую копию БД?",
                       "Обновление БД", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (answer2 != MessageBoxResult.Yes) return;
                    File.Delete(result.Item2);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Ошибка при обновлении БД");
                    _data.IsDataBaseConnect = false;
                }
            });
            NavigateToPathDB = new RelayCommand(() =>
            {
                string? directoryPath = Path.GetDirectoryName(DbPath);
                if (Directory.Exists(directoryPath))
                {
                    Process.Start("explorer.exe", directoryPath);
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

        private async Task<bool> TryConnectAsync(string userDbPath)
        {
            try
            {
                bool IsGoodDb = await RunChecksAsync(_userDbPath);
                if (IsGoodDb == false)
                {
                    Growl.Error(CheckError);
                    return false;
                }

                string newDbPath = _userDbPath;
                _dbProvider.ChangeDataBase(newDbPath);

                DbPath = _userDbPath;
                DbName = Path.GetFileName(DbPath);
                Growl.Success("Выбрана новая База Данных");
                _data.IsDataBaseConnect = true;
                AppPaths.SaveDbPath(newDbPath);
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Ошибка при изменение БД", System.Windows.MessageBoxButton.OK);
                _data.IsDataBaseConnect = false;
                return false;
            }
        }

        private async Task<bool> RunChecksAsync(string path)
        {
            try
            {
                Steps.Clear();
                CheckError = null;
                StepIndex = 0;

                var checks = _dbProvider.CreateChecks();
                foreach (var c in checks)
                    Steps.Add(new CheckStepVM(c));

                foreach (var step in Steps)
                {
                    step.State = StepState.Запущен;

                    CheckResult result;
                    try { result = await step.Check.RunAsync(path); }
                    catch (Exception ex) { result = new CheckResult(false, ex.Message); }

                    if (!result.Ok)
                    {
                        step.State = StepState.Ошибка;
                        step.Message = result.Message;
                        CheckError = $"Ошибка: {result.Message}";
                        if (result.Kind == FailureKind.NeedsMigration) ShowUpdateDbButton = true;
                        return false;
                    }

                    step.State = StepState.Успешно;
                    StepIndex++;
                }
                return true;
            }

            finally
            {
                SqliteConnection.ClearAllPools();
            }
        }
        
        public ICommand ChangeDataBase { get; private set; }
        public ICommand NavigateToPathDB { get; }
        public ICommand LoadMoreLogs { get; }
        public ICommand CopyLog { get; }
        public ICommand ShowFullLog { get; }
        public ICommand UpdateDataBaseMigration { get; }
    }
}
