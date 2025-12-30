using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Properties;
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
        private string passwordEditor;
        [Reactive]
        public string PasswordEditor
        {
            get => passwordEditor;
            set { this.RaiseAndSetIfChanged(ref passwordEditor, value); }
        }
        
        public SettingsVM()
        {
            ChangeEditorPassword = new RelayCommand<string>((newPass) => 
            {
                Settings.Default.EditorPassword = newPass;
                Settings.Default.Save();
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
