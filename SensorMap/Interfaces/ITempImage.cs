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
        Uri GetUriFromBytes(byte[] imageData, string fileName = "image");
        public void Cleanup();
    }
}
