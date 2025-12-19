using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class SettingsVM:ReactiveObject
    {
        private IConfiguration config;
        private string passwordEditor;
        [Reactive]
        public string PasswordEditor
        {
            get => passwordEditor;
            set { this.RaiseAndSetIfChanged(ref passwordEditor, value); }
        }
        
        public SettingsVM(IConfiguration _config)
        {
            config = _config;
            ChangeEditorPassword = new RelayCommand<string>((newPass) => 
            {
                config["SecurityData:Editor:Password"] = newPass;
                Growl.Warning(new GrowlInfo
                {
                    Message = "Изменен пароль для Редактора БД!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            });
        }

        public ICommand ChangeEditorPassword { get;private set; }
    }
}
