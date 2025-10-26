using SensorMap.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SensorMap.Services
{
    public class TempImage : ITempImage,IDisposable
    {
        private readonly Dictionary<string, string> _tempFiles = new Dictionary<string, string>();

        public Uri GetUriFromBytes(byte[] imageData, string fileName = "image")
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            var hash = ComputeHash(imageData);
            if (_tempFiles.TryGetValue(hash, out var existingFile) && File.Exists(existingFile))
            {
                return new Uri(existingFile);
            }

            var tempFile = Path.Combine(Path.GetTempPath(), $"{fileName}_{Guid.NewGuid()}.jpg");
            File.WriteAllBytes(tempFile, imageData);
            _tempFiles[hash] = tempFile;

            return new Uri(tempFile);
        }

        public void Cleanup()
        {
            foreach (var tempFile in _tempFiles.Values)
            {
                try
                {
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);
                }
                catch { }
            }
            _tempFiles.Clear();
        }

        private string ComputeHash(byte[] data)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(data);
            return Convert.ToBase64String(hash);
        }

        public void Dispose() => Cleanup();

        public ImageSource CreateImageFromBytes(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;

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
    }
}
