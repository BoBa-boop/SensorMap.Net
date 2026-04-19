using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.EF
{
    public class DbActionLogs
    {
        public DateTime Date { get; set; }
        public string Action { get; set; }
        public string SqlQuery { get; set; }
    }
}
