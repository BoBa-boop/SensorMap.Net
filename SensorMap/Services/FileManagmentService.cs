using SensorMap.Interfaces;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SensorMap.Services
{
    public class FileManagmentService : IFileManagment
    {
        public string[] OpenFileDialog()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.SelectReadOnly = true;
            fileDialog.Multiselect = true;
            fileDialog.ShowDialog();
            if (fileDialog.FileNames.Any())
            {
                return fileDialog.FileNames.ToArray();
            }
            else
                return Array.Empty<string>();

        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr obj);
        public ImageSource GetIconFile(string path)
        {
            if (!File.Exists(path)) return new BitmapImage();

            using(System.Drawing.Icon sysIcon = System.Drawing.Icon.ExtractAssociatedIcon(path))
            {
                if(sysIcon == null) return new BitmapImage();

                ImageSource img = Imaging.CreateBitmapSourceFromHIcon
                    (
                    sysIcon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                return img;
            }
        }
    }
}
