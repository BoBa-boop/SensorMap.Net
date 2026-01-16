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
        private readonly ObservableAsPropertyHelper<bool> _isAuthHelper;
        private readonly ObservableAsPropertyHelper<string> _messageHelper;

        public bool IsAuth => _isAuthHelper.Value;
        public string UIMessageState => _messageHelper.Value;




        public AuthVM(IDataService _data,IAuthorization authorization) 
        {
            dataService = _data;
            _authorization = authorization;

            authorization.WhenAnyValue(x => x.IsSuccessAuth)
                     .ToProperty(this, x => x.IsAuth, out _isAuthHelper);

            authorization.WhenAnyValue(x => x.MessageState)
                         .ToProperty(this, x => x.UIMessageState, out _messageHelper);

            VerifyCommand = ReactiveCommand.Create<object>((obj) =>
            {
                if (obj is PasswordBox pBox)
                {
                    _authorization.Authorization(pBox.Password);
                    if (IsAuth == false) pBox.Password = string.Empty;
                }
            });
            this.WhenAnyValue(x => x.IsAuth)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe( isAuth => dataService.IsEditMode = isAuth);
           
        }
        public ICommand VerifyCommand { get; set; }
    }
}
