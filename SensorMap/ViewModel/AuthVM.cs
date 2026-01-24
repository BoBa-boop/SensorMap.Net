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
        private readonly ObservableAsPropertyHelper<string> _messageHelper;

        public string UIMessageState => _messageHelper.Value;
        [Reactive]public bool IsAuth 
        {
            get => _isAuth;
            set
            {
                this.RaiseAndSetIfChanged(ref _isAuth, value);
            }
        }



        public AuthVM(IDataService _data,IAuthorization authorization) 
        {
            dataService = _data;
            _authorization = authorization;

            authorization.WhenAnyValue(x => x.MessageState)
                         .ToProperty(this, x => x.UIMessageState, out _messageHelper);

            VerifyCommand = ReactiveCommand.Create<object>((obj) =>
            {
                if (obj is PasswordBox pBox)
                {
                    IsAuth = _authorization.Authorization(pBox.Password);
                    if(IsAuth) _data.IsEditMode = true;
                    if (IsAuth == false) pBox.Password = string.Empty;
                }
            });
           
        }
        public ICommand VerifyCommand { get; set; }
        
    }
}
