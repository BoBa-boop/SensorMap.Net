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
    public sealed class AuthorizationService(IPasswordHash passwordHash) : ReactiveObject,IAuthorization
    {
        private string messageState = "Введите пароль";

        [Reactive] public string MessageState
        {
            get => messageState;
            private set => this.RaiseAndSetIfChanged(ref messageState, value);
        }

        public bool IsPasswordSet => !string.IsNullOrEmpty(Settings.Default.EditorPassword);

        public bool Authorization(string password)
        {
            if (passwordHash.Verify(password, Settings.Default.EditorPassword)) return true;
            else
            {
                password = string.Empty;
                MessageState = "Неверный пароль!";
                return false;
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

        public IReadOnlyList<string> GenerateRecoveryCodes()
        {
            var codes = Enumerable.Range(0, 10)
                .Select(_ => Convert.ToHexString(RandomNumberGenerator.GetBytes(6))
                ).ToList();
            Settings.Default.RecoveryCodes = string.Join(";", codes.Select(passwordHash.Hash));
            Settings.Default.Save();
            return codes;
        }

        public bool VerifyRecoveryCode(string code)
        {
            var normalized = code.Replace("-", "").Replace(" ", "").ToUpperInvariant();
            var hashes = Settings.Default.RecoveryCodes
                .Split(";",StringSplitOptions.RemoveEmptyEntries).ToList();
            var hit = hashes.FirstOrDefault(h => passwordHash.Verify(normalized, h));
            if(hit is null) return false;

            hashes.Remove(hit);
            Settings.Default.RecoveryCodes = string.Join(";",hashes);
            Settings.Default.Save();
            return true;

        }
    }
}
