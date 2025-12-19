using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.View;
using System.Windows;

namespace SensorMap.Services
{
    public class NavigationService : ReactiveObject, INavigation
    {
        private Func<Type, ReactiveObject> _vmFact;
        private readonly Func<Type, object, ReactiveObject> _vmFactWithParam;
        private Window? _mainWindow;
        private IServiceProvider _serviceProvider;
        private ReactiveObject? _currentView;
        public ReactiveObject CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }
        public NavigationService(Func<Type, ReactiveObject> vmFact, Func<Type, object, ReactiveObject> vmFactWithParam,IServiceProvider provider)
        {
            _serviceProvider= provider;
            _vmFact = vmFact;
            _vmFactWithParam = vmFactWithParam;
        }
        public void NavigateTo<T>()
        {
            ReactiveObject viewModel = _vmFact.Invoke(typeof(T));
            CurrentView = viewModel;
        }

        public void NavigateTo<T>(object parameter)
        {
            ReactiveObject viewModel = _vmFactWithParam.Invoke(typeof(T), parameter);
            CurrentView = viewModel;
        }
        public void SetMainWindow(Window window)
        {
            _mainWindow = window;
        }
        public void ShowDialog<TWindow, TViewModel>() where TWindow : Window where TViewModel : class
        {
            if (_mainWindow == null) return;

            var dialogWindow = _serviceProvider.GetRequiredService<TWindow>();
            var viewModel = _vmFact.Invoke(typeof(TViewModel));
            dialogWindow.ShowInTaskbar = false;
            dialogWindow.DataContext = viewModel;

            dialogWindow.ShowDialog();
        }

        public void ShowDialog<TWindow, TViewModel>(object parameter)
            where TWindow : Window
            where TViewModel : class
        {
            if (_mainWindow == null) return;

            var dialogWindow = _serviceProvider.GetRequiredService<TWindow>();
            var viewModel = _vmFactWithParam.Invoke(typeof(TViewModel), parameter);

            dialogWindow.DataContext = viewModel;
            dialogWindow.Owner = _mainWindow;
            _mainWindow.IsEnabled = false;

            dialogWindow.Closed += (s, args) =>
            {
                _mainWindow.IsEnabled = true;
                _mainWindow.Activate();
            };

            dialogWindow.ShowDialog();
        }
    }
}
