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
    /// Логика взаимодействия для MainMenu.xaml
    /// </summary>
    public partial class MainMenu : System.Windows.Controls.UserControl,IViewFor<MainMenuVM>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register
           (
           nameof(ViewModel),
           typeof(MainMenuVM),
           typeof(MainMenu),
           new PropertyMetadata(null));
        public MainMenuVM? ViewModel
        {
            get => (MainMenuVM?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (MainMenuVM?)value; }
        public MainMenu()
        {
            InitializeComponent();
            DataContextChanged += (_, _) => ViewModel = DataContext as MainMenuVM;
            carousel.Loaded += (s, e) =>
            {
                carousel.Items.Clear();
                foreach (var item in ViewModel.MySettings.MenuImages)
                {
                    carousel.Items.Add(new HandyControl.Controls.CarouselItem()
                    {
                        DataContext = item,
                        Content = item
                    });
                }
            };
        }

    }
}
