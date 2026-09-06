using ReactiveUI;
using SensorMap.ViewModel;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace SensorMap.View
{
    public partial class SectorView : UserControl, IViewFor<SectorsVM>
    {
        private CompositeDisposable? _activationDisposables = new CompositeDisposable();
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register
           (
           nameof(ViewModel),
           typeof(SectorsVM),
           typeof(SectorView),
           new PropertyMetadata(null));
        public SectorsVM? ViewModel
        {
            get => (SectorsVM?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (SectorsVM?)value; }
        public SectorView()
        {
            InitializeComponent();
            DataContextChanged += (_, _) => ViewModel = DataContext as SectorsVM;
            this.WhenActivated(disposables =>
            { // Здесь можно подписываться на команды самого Окна, если они нужны 
              // Например: this.BindCommand(ViewModel, vm => vm.SomeCmd, v => v.FindName<Button>("MyBtn"))
              // .DisposeWith(disposables);
            }).DisposeWith(_activationDisposables);
        }

        
    }
}
