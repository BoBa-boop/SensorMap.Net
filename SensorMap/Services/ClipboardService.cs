using SensorMap.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Services
{
    public class ClipboardService : IClipboard
    {
        private object? _clipboardData;

        public void Copy<T>(T data)
        {
            _clipboardData = data;
        }

        public T? Paste<T>()
        {
            return (T)_clipboardData??default;
        }
    }
}
