using ReactiveUI;
using System.Windows;

namespace SensorMap.Interfaces
{
    public interface INavigation
    {
        ReactiveObject CurrentView { get; set; }
        void NavigateTo<T>();
        void NavigateTo<T>(object parameter);
        void ShowDialog<TWindow,TViewModel>() where TWindow : Window where TViewModel : class;
        void ShowDialog<TWindow, TViewModel>(object parameter) where TWindow : Window where TViewModel : class;
        void SetMainWindow(Window mainWindow);
    }
}
