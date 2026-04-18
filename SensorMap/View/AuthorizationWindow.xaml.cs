
using SensorMap.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace SensorMap.View
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            InitializeComponent();
            pinBox.Focus();
        }

    }
}
