using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SensorMap.Interfaces
{
    public interface ITempImage
    {
        ImageSource CreateImageFromBytes(byte[] imageData);
        bool IsImage(byte[] imageData);
        byte[] OpenImageDialog();
        Uri GetUriFromBytes(byte[] imageData, string fileName = "image");
        public void Cleanup();
        public byte[] ConvertToByte(ImageSource image);
        void OpenFullScreen(ImageSource path);
    }
}
