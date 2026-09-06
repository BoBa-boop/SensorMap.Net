using HandyControl.Controls;
using ReactiveUI;
using SensorMap.ViewModel;
using System.Reactive.Disposables;
using System.Windows;

namespace SensorMap.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : HandyControl.Controls.Window,IViewFor<MainWindowVM>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register
            (
            nameof(ViewModel),
            typeof(MainWindowVM),
            typeof(MainWindow),
            new PropertyMetadata(null));
        public MainWindowVM? ViewModel 
        {
            get => (MainWindowVM?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        } 
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (MainWindowVM?)value; } 
        public MainWindow() 
        { InitializeComponent();
            DataContextChanged += (_, _) => ViewModel = DataContext as MainWindowVM;
            this.WhenActivated(disposables => 
            { // Здесь можно подписываться на команды самого Окна, если они нужны 
              // Например: this.BindCommand(ViewModel, vm => vm.SomeCmd, v => v.FindName<Button>("MyBtn"))
              // .DisposeWith(disposables);
            });
        }


    }
}
