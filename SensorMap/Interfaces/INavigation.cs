using ReactiveUI;
using System.Windows;

namespace SensorMap.Interfaces
{
    public interface INavigation
    {
        ReactiveObject CurrentView { get; set; }
        void NavigateTo<T>();
        void NavigateTo<T>(object parameter);
        void ShowDialog<TWindow>() where TWindow : Window;
        void SetMainWindow(Window mainWindow);
    }
}
