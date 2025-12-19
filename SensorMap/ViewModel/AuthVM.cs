using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
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
        private readonly IConfiguration configuration;
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
        public AuthVM(IDataService _data,IConfiguration config) 
        {
            configuration = config;
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
            if (string.IsNullOrEmpty(configuration["SecurityData:Editor:Password"]))
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
            if (uiPass == configuration["SecurityData:Editor:Password"]) IsAuth = true;
        }
        public ICommand VerifyCommand { get; set; }
    }
}
