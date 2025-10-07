using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.EF;
using SensorMap.Interfaces;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class MenuVM:ReactiveObject
    {
        private IDataService _dataService;
        [Reactive] public INavigation Navigation { get; set; }

        private bool _isEdit;
        [Reactive]
        public bool IsEditMode
        {
            get => _isEdit;
            set => this.RaiseAndSetIfChanged(ref _isEdit, value);
        }
        public MenuVM(IDataService dataService, INavigation _nav)
        {
            Navigation = _nav;
            _dataService = dataService;

            ShowMenu = new RelayCommand(() => Navigation.NavigateTo<MenuButtonsVM>());

            
            _dataService.WhenAnyValue(x => x.IsEditMode)
                        .ObserveOn(RxApp.MainThreadScheduler)
                       .Subscribe(value => IsEditMode = value);

            this.WhenAnyValue(vm => vm.IsEditMode)
                 .ObserveOn(RxApp.MainThreadScheduler)
               .Subscribe(value => _dataService.IsEditMode=value);

        }


        public ICommand ShowMenu { get; set; }
    }
}
