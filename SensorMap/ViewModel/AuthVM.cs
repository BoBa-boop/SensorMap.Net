using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class AuthVM:ReactiveObject
    {
        private IDataService dataService;
        bool _isAuth = false;
        private string _pass;
        private string _state = "Пароль";

        [Reactive]
        public bool IsAuth
        {
            get => _isAuth;
            set { this.RaiseAndSetIfChanged(ref _isAuth, value); }
        }
        
        [Reactive]
        public string UIMessageState
        {
            get => _state;
            set 
            {
                this.RaiseAndSetIfChanged(ref _state, value); 
            }
        }
        public AuthVM(IDataService _data) 
        {
            dataService = _data;
            VerifyCommand = new RelayCommand<object>((obj) =>
            {
                CheckEditorPassword(obj);
            });
            this.WhenAnyValue(x => x.IsAuth).ObserveOn(RxApp.MainThreadScheduler).Subscribe((s) =>
            {
                dataService.IsEditMode = IsAuth;
            });
           
        }

        private void CheckEditorPassword(object? obj)
        {
            var pass = obj as PasswordBox;
            if (string.IsNullOrEmpty(Settings.Default.EditorPassword))
            {
                pass.Password = string.Empty;
                UIMessageState = "Создайте пароль в настройках!";
                return;
            }
            VerifyUser(pass.Password);
            if (!IsAuth)
            {
                pass.Password = string.Empty;
                UIMessageState = "Неверный пароль!";
            }
        }

        private void VerifyUser(string uiPass)
        {
            if (uiPass == Settings.Default.EditorPassword) IsAuth = true;
        }
        public ICommand VerifyCommand { get; set; }
    }
}
