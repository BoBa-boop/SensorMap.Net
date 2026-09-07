using ReactiveUI;
using SensorMap.ViewModel;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace SensorMap.View
{
    /// <summary>
    /// Логика взаимодействия для CRUD_View.xaml
    /// </summary>
    public partial class CRUD_View : UserControl, IViewFor<CRUD_VM>
    {
        private CompositeDisposable? _activationDisposables = new CompositeDisposable();
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register
           (
           nameof(ViewModel),
           typeof(CRUD_VM),
           typeof(CRUD_View),
           new PropertyMetadata(null));
        public CRUD_VM? ViewModel
        {
            get => (CRUD_VM?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (CRUD_VM?)value; }
        public CRUD_View()
        {
            InitializeComponent();
            DataContextChanged += (_, _) => ViewModel = DataContext as CRUD_VM;
            this.WhenActivated(disposables =>
            { // Здесь можно подписываться на команды самого Окна, если они нужны 
              // Например: this.BindCommand(ViewModel, vm => vm.SomeCmd, v => v.FindName<Button>("MyBtn"))
              // .DisposeWith(disposables);
            }).DisposeWith(_activationDisposables);
        }

        private void DataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            var grid = (DataGrid)sender;
            grid.CancelEdit(DataGridEditingUnit.Row);
        }
    }
}
