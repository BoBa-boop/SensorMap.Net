using ReactiveUI;
using SensorMap.ViewModel;
using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace SensorMap.View
{
    /// <summary>
    /// Логика взаимодействия для SensorView.xaml
    /// </summary>
    public partial class SensorView : UserControl,IViewFor<SensorVM>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register
           (
           nameof(ViewModel),
           typeof(SensorVM),
           typeof(SensorView),
           new PropertyMetadata(null));
        public SensorVM? ViewModel
        {
            get => (SensorVM?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (SensorVM?)value; }
        public SensorView()
        {
            InitializeComponent();
            DataContextChanged += (_, _) => ViewModel = DataContext as SensorVM;
            this.WhenActivated(disposables =>
            { // Здесь можно подписываться на команды самого Окна, если они нужны 
              // Например: this.BindCommand(ViewModel, vm => vm.SomeCmd, v => v.FindName<Button>("MyBtn"))
              // .DisposeWith(disposables);
            });
        }
    }
}
