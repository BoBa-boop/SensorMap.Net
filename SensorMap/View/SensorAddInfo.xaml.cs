using HandyControl.Controls;
using HandyControl.Data;
using SensorMap.Behaviors;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Button = System.Windows.Controls.Button;
using UserControl = System.Windows.Controls.UserControl;

namespace SensorMap.View
{
    /// <summary>
    /// Логика взаимодействия для test.xaml
    /// </summary>
    public partial class SensorAddInfo : UserControl
    {
        public SensorAddInfo()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if(sender is Button button)
            {
                var sensor = button.DataContext as SensorAssignments;
                var image = sensor.Image;
                var browser = new CustomImageBrowser(CreateImageFromBytes(image)) { Title = "Реальное расположение" };
                browser.ShowDialog();
            }
        }
        private ImageSource CreateImageFromBytes(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            try
            {
                using (var stream = new MemoryStream(imageData))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch
            {
                return new BitmapImage();
            }
            
        }
    }
}
