using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Model
{
    public class AppSettings
    {
        public string ConnectionStrings { get; set; } = string.Empty;
        public string InterfaceSettings { get; set; } = string.Empty;
        public string SecurityData { get; set; } = string.Empty;
    }
}
