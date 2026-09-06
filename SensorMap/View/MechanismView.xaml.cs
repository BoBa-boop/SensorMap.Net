using ReactiveUI;
using SensorMap.Model;
using SensorMap.ViewModel;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace SensorMap.View
{
    /// <summary>
    /// Логика взаимодействия для MechanismView.xaml
    /// </summary>
    public partial class MechanismView : UserControl,IViewFor<MechanismVM>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register
           (
           nameof(ViewModel),
           typeof(MechanismVM),
           typeof(MechanismView),
           new PropertyMetadata(null));
        public MechanismVM? ViewModel
        {
            get => (MechanismVM?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (MechanismVM?)value; }
        public MechanismView()
        {
            InitializeComponent();
            DataContextChanged += (_, _) => ViewModel = DataContext as MechanismVM;
            this.WhenActivated(disposables =>
            { // Здесь можно подписываться на команды самого Окна, если они нужны 
              // Например: this.BindCommand(ViewModel, vm => vm.SomeCmd, v => v.FindName<Button>("MyBtn"))
              // .DisposeWith(disposables);
            });
        }

       

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                if (e.Delta > 0)
                    scrollViewer.LineUp();
                else
                    scrollViewer.LineDown();
                e.Handled = true;
            }
        }
    }
}
