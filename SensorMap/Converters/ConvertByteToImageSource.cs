using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SensorMap.Converters
{
    class ConvertByteToImageSource : IValueConverter
    {
        private static readonly Dictionary<string, string> _tempFiles = new Dictionary<string, string>();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not byte[] imageData || imageData.Length == 0)
                return null;

            try
            {
                // Создаем временный файл
                string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jpg");
                File.WriteAllBytes(tempFile, imageData);

                // Сохраняем ссылку для последующего удаления
                string key = GetImageKey(imageData);
                _tempFiles[key] = tempFile;

                // Возвращаем URI для ImageViewer
                return new Uri(tempFile);
            }
            catch
            {
                return null;
            }
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        public static void CleanupTempFile(byte[] imageData)
        {
            if (imageData == null) return;

            string key = GetImageKey(imageData);
            if (_tempFiles.TryGetValue(key, out string tempFile))
            {
                try
                {
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);

                    _tempFiles.Remove(key);
                }
                catch
                {
                    // Игнорируем ошибки удаления
                }
            }
        }

        private static string GetImageKey(byte[] imageData)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(imageData);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
