using HandyControl.Controls;
using ReactiveUI;
using SensorMap.ViewModel;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UserControl = System.Windows.Controls.UserControl;

namespace SensorMap.View
{
    /// <summary>
    /// Логика взаимодействия для CRUD_View.xaml
    /// </summary>
    public partial class CRUD_View : UserControl, IViewFor<CRUD_VM>
    {
        private readonly List<DataGrid> dataGrids = new();
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

        private void root_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                foreach (DataGrid dataGrid in dataGrids)
                {
                    var view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
                    if (view is IEditableCollectionView ecv)
                    {
                        if (ecv.IsAddingNew) ecv.CancelNew();
                        if (ecv.IsEditingItem) ecv.CancelEdit();
                    }
                    dataGrid.CancelEdit(DataGridEditingUnit.Cell);
                    dataGrid.CancelEdit(DataGridEditingUnit.Row);
                }
            }
            catch
            {
                Growl.Error("Возникла ошибка при отмене редактирования таблицы. Перезагрузите вкладку");
            }
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var dg = (DataGrid)sender;
            if(!dataGrids.Contains(dg))dataGrids.Add(dg);
        }
    }
}
