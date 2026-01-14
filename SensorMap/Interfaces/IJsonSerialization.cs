
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Interfaces
{
    public interface IJsonSerialization
    {
        void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false);
        T ReadFromJsonFile<T>(string filePath);
    }
}
