using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Interfaces
{
    public interface IClipboard
    {
        /// <summary>
        /// Копирует данные в буфер обмена.
        /// </summary>
        void Copy<T>(T data);

        /// <summary>
        /// Вставляет данные из буфера обмена.
        /// </summary>
        T? Paste<T>();
    }
}
