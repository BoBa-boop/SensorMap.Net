using HandyControl.Controls;
using HandyControl.Data;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Services
{
    public sealed class AuthorizationService(IDataService data,IPasswordHash passwordHash) : ReactiveObject,IAuthorization
    {
        private string messageState = "Введите пароль";

        [Reactive] public string MessageState
        {
            get => messageState;
            private set => this.RaiseAndSetIfChanged(ref messageState, value);
        }

        public bool Authorization(string password)
        {
            ErrorMessages();
            if (passwordHash.Verify(password, Settings.Default.EditorPassword)) return true;
            else
            {
                password = string.Empty;
                MessageState = "Неверный пароль!";
                return false;
            }
        }

        private void ErrorMessages()
        {
            if (string.IsNullOrEmpty(Settings.Default.EditorPassword))
            {
                Growl.Warning(new GrowlInfo
                {
                    Message = "Создайте пароль в Настройках!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
                return;
            }
        }
        

        public void ChangePassword(string password)
        {
            Settings.Default.EditorPassword = passwordHash.Hash(password);
            Settings.Default.Save();
            Growl.Warning(new GrowlInfo
            {
                Message = "Изменен пароль для Редактора БД!",
                CancelStr = "Ignore",
                ShowDateTime = false,
                WaitTime = 2
            });
        }
    }
}
