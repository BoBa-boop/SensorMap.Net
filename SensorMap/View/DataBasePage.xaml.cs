using ReactiveUI;
using SensorMap.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SensorMap.View
{
    /// <summary>
    /// Логика взаимодействия для DataBasePage.xaml
    /// </summary>
    public partial class DataBasePage : System.Windows.Controls.UserControl,IViewFor<DataBasePageVM>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register
            (
            nameof(ViewModel),
            typeof(DataBasePageVM),
            typeof(DataBasePage),
            new PropertyMetadata(null));
        public DataBasePageVM? ViewModel
        {
            get => (DataBasePageVM?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (DataBasePageVM?)value; }
        public DataBasePage()
        {
            InitializeComponent();
            DataContextChanged += (_, _) => ViewModel = DataContext as DataBasePageVM;
            this.WhenActivated(disposables =>
            { // Здесь можно подписываться на команды самого Окна, если они нужны 
              // Например: this.BindCommand(ViewModel, vm => vm.SomeCmd, v => v.FindName<Button>("MyBtn"))
              // .DisposeWith(disposables);
            });
        }
    }
}
