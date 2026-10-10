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
using SensorMap.Services.UpdateService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace SensorMap.ViewModel
{
    public class SettingsVM:ReactiveObject
    {
        private readonly Updater _updater = new();
        private IAuthorization _auth;
        private IJsonSerialization _json;
        private SettingsApp settings;

        [Reactive]public SettingsApp MySettings { get => settings; set => settings = value; }
        public string VersionText => _updater.CurrentVersion is { } v ? $"Версия {v}" : "Версия: режим отладки";
        private bool isBusy;

        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value; }
        }
        private int progress;

        public int Progress
        {
            get { return progress; }
            set { progress = value; }
        }
        private string updStatus;

        [Reactive]public string UpdateStatus
        {
            get { return updStatus; }
            set => this.RaiseAndSetIfChanged(ref updStatus, value);
        }
        public SettingsVM(IAuthorization authorization, IJsonSerialization jsonSerialization)
        {
            _auth = authorization;
            _json = jsonSerialization;

            if (Path.Exists(SettingsApp.SettingsPath)) MySettings = _json.ReadFromJsonFile<SettingsApp>(SettingsApp.SettingsPath);
            else 
            {
                MySettings = new SettingsApp();
                MySettings.MenuImages = new(new[] { "", "", "" });
                _json.WriteToJsonFile<SettingsApp>(SettingsApp.SettingsPath, MySettings);
            }

            CreateRecoveryCodes = new RelayCommand(() => _auth.GenerateRecoveryCodes());
            SaveMenuImages = new RelayCommand(() => _json.WriteToJsonFile(SettingsApp.SettingsPath, MySettings));
            CheckUpdateCommand = new RelayCommand(async () => await CheckForUpdates(askUser: true));
        }

        public ICommand CreateRecoveryCodes { get;private set; }
        public ICommand SaveMenuImages { get; }
        public ICommand CheckUpdateCommand { get; }

        public async Task CheckForUpdates(bool askUser)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                UpdateStatus = "Проверка обновления";
                if (!_updater.IsInstalled)
                {
                    UpdateStatus = "Обновление работают только в установленной версии";
                    return;
                }
                if (!await _updater.CheckAsync())
                {
                    UpdateStatus = "Установлена последняя версия";
                    return;
                }
                var question = $"Доступна новая версия {_updater.NewVersion}. Установить сейчас?";
                if (askUser && HandyControl.Controls.MessageBox.Show(question, "Обновление", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question)
                    != System.Windows.MessageBoxResult.Yes)
                {
                    UpdateStatus = $"Доступна новая версия {_updater.NewVersion}";
                    return;
                }
                UpdateStatus = "Скачивание...";
                await _updater.DownloadAsync(p => App.Current.Dispatcher.Invoke(() => Progress = p));
                UpdateStatus = "Установка, приложение перезапуститься...";
                _updater.ApplyAndRestart();
            }
            catch (Exception ex)
            {
                UpdateStatus = "Не удалось проверить обновления";

            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
