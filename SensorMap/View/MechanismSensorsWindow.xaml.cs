using SensorMap.Model;
using System.Windows.Controls;

namespace SensorMap.View
{
    /// <summary>
    /// Логика взаимодействия для MechanismSensorsWindow.xaml
    /// </summary>
    public partial class MechanismSensorsWindow : HandyControl.Controls.Window
    {
        public MechanismSensorsWindow()
        {
            InitializeComponent();
        }

        private void SensorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox cb && cb.DataContext is SensorAssignments sa && cb.SelectedItem is Sensor newSensor)
            {
                sa.Sensor = newSensor;
            }
        }
    }
}
