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
using UserControl = System.Windows.Controls.UserControl;

namespace SensorMap.View
{
    /// <summary>
    /// Логика взаимодействия для PLC_View.xaml
    /// </summary>
    public partial class Devices_View : UserControl, IViewFor<Devices_VM>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register
           (
           nameof(ViewModel),
           typeof(Devices_VM),
           typeof(Devices_View),
           new PropertyMetadata(null));
        public Devices_VM? ViewModel
        {
            get => (Devices_VM?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (Devices_VM?)value; }
        public Devices_View()
        {
            InitializeComponent(); 
            DataContextChanged += (_, _) => ViewModel = DataContext as Devices_VM;
            this.WhenActivated(disposables =>
            { 
            });
        }
    }
}
