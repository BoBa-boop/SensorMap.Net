using HandyControl.Controls;
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
    /// Логика взаимодействия для SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl,IViewFor<SettingsVM>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register
           (
           nameof(ViewModel),
           typeof(SettingsVM),
           typeof(SettingsPage),
           new PropertyMetadata(null));
        public SettingsVM? ViewModel
        {
            get => (SettingsVM?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (SettingsVM?)value; }
        public SettingsPage()
        {
            InitializeComponent();
            DataContextChanged += (_, _) => ViewModel = DataContext as SettingsVM;
        }

        private void ImageSelector_ImageSelected(object sender, RoutedEventArgs e)
        {
            var selector = sender as ImageSelector;
            int index = int.Parse(selector.Tag.ToString());
            string pathImage = selector.Uri.OriginalString;
            ViewModel.MySettings.MenuImages[index] = pathImage;
        }
    }
}
