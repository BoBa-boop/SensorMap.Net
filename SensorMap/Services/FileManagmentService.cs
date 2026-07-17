using HandyControl.Controls;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SensorMap.Services
{
    public class FileManagmentService : IFileManagment
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public string[] OpenFileDialog(bool multiselect = false)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.SelectReadOnly = true;
            fileDialog.Multiselect = multiselect;
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

        public bool AddHelpfulFile(ITempImage imgManag,object Entity, bool multiselect = false)
        {
            string[] paths = OpenFileDialog(multiselect);
            if (paths.Length <= 0) return false;
            foreach (var path in paths)
            {
                if (Entity is Device device) 
                {
                    device.Files.Add(new HelpfulFile()
                    {
                        DeviceId = device.Id,
                        NameFile = path,
                        ImageFile = imgManag.ConvertToByte(GetIconFile(path)),
                        IsNew = true
                    });
                }
                
                if(Entity is Mechanism mechanism)
                {
                    mechanism.Files.Insert(0,new HelpfulFile()
                    {
                        MechanismId = mechanism.Id,
                        NameFile = path,
                        ImageFile = imgManag.ConvertToByte(GetIconFile(path))
                    });
                }
                if (Entity is Sensor sensor)
                {
                    sensor.Files.Add(new HelpfulFile()
                    {
                        SensorId = sensor.Id,
                        NameFile = path,
                        ImageFile = imgManag.ConvertToByte(GetIconFile(path)),
                        IsNew = true
                    });
                }
            }
            return true;
        }

        public bool OpenFileInExplorer(string path)
        {
            try
            {
                if(!File.Exists(path)) throw new FileNotFoundException();
                Process.Start("explorer.exe", "/select,\"" + Path.GetFullPath(path) + "\"");
                return true;
            }
            catch (Exception ex) 
            {
                Growl.Error("Не удалось открыть файл в проводнике");
                Logger.Error(ex.Message);
                return false;
            }
        }

    }
}
