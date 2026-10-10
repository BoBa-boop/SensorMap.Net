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
using System.Reactive.Disposables;
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
        private IAuthorization _authorization;
        private bool _isAuth;
        private string? _pending;
        private string uIMessageState;

        [Reactive] public string UIMessageState { get => uIMessageState; set => this.RaiseAndSetIfChanged(ref uIMessageState, value); }
        [Reactive]public bool IsAuth 
        {
            get => _isAuth;
            set
            {
                this.RaiseAndSetIfChanged(ref _isAuth, value);
            }
        }
        public enum AuthMode { Login,CreatePassword, ConfirmPassword,EnterRecoveryCode }
        [Reactive] public AuthMode Mode { get; set; }
        public bool IsLoginMode => Mode == AuthMode.Login;


        public AuthVM(IDataService _data,IAuthorization authorization) 
        {
            dataService = _data;
            _authorization = authorization;

            if (_authorization.IsPasswordSet) 
            {
                Mode = AuthMode.Login; 
                UIMessageState = "Введите пароль"; 
            }
            else
            {
                Mode = AuthMode.CreatePassword;
                UIMessageState = "Создайте пароль";
            }

            VerifyCommand = ReactiveCommand.Create<object>((obj) =>
            {
                if (obj is not System.Windows.Controls.TextBox box) return;
                var text = box.Text;
                box.Text = string.Empty;
                switch (Mode)
                {
                    case AuthMode.Login:
                        if (_authorization.Authorization(text)) Succeed();
                        else UIMessageState = "Неверный пароль!";
                        break;
                    case AuthMode.CreatePassword:
                        if (text.Length < 4)
                        {
                            UIMessageState = "Минимум 4 символа";
                            break;
                        }
                        _pending = text;
                        Mode = AuthMode.ConfirmPassword;
                        UIMessageState = "Повторите пароль";
                        break;
                    case AuthMode.ConfirmPassword:
                        if (text != _pending)
                        {
                            _pending = null;
                            Mode = AuthMode.CreatePassword;
                            UIMessageState = "Не совпали пароли. Создайте пароль";
                            break;
                        }
                        _authorization.ChangePassword(text);
                        _authorization.GenerateRecoveryCodes();
                        Succeed();
                        break;
                    case AuthMode.EnterRecoveryCode:
                        if (_authorization.VerifyRecoveryCode(text))
                        {
                            Mode = AuthMode.CreatePassword;
                            UIMessageState = "Введите новый пароль";
                        }
                        else UIMessageState = "Неверный код!";
                        break;
                }
                
                
            });
            void Succeed() 
            { 
                IsAuth = true;
                dataService.IsEditMode = true;
                if (string.IsNullOrEmpty(Settings.Default.RecoveryCodes))
                {
                    var ask = HandyControl.Controls.MessageBox.Show("Коды восстановления пароля не созданы. Создать сейчас?",
                        "Восстановление пароля",MessageBoxButton.YesNo);
                    if (ask == MessageBoxResult.Yes) _authorization.GenerateRecoveryCodes();
                }
                if (_authorization.CountRecoveryCode() < 2) 
                {
                    var ask = HandyControl.Controls.MessageBox.Show("Осталось мало кодов восстановления пароля. Создать новые коды?",
                        "Пароль", MessageBoxButton.YesNo);
                    if (ask == MessageBoxResult.Yes) _authorization.GenerateRecoveryCodes();
                }
            }
            ForgotCommand = ReactiveCommand.Create(() =>
            {
                if (string.IsNullOrEmpty(Settings.Default.RecoveryCodes))
                {
                    UIMessageState = _authorization.IsPasswordSet? "Коды восстановления не созданы":"Создайте новый пароль";
                    return;
                }
                Mode = AuthMode.EnterRecoveryCode;
                UIMessageState = "Введите код восстановления";
            });
            
        }
        public ICommand VerifyCommand { get; set; }
        public ICommand ForgotCommand { get; set; }
    }
}
