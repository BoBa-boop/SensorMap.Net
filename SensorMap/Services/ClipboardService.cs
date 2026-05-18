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

        public T? Cut<T>(ref T? source)
        {
            _clipboardData = source;
            source = default;
            return (T)_clipboardData??default;
        }

        public T? Paste<T>()
        {
            return (T)_clipboardData??default;
        }
    }
}
