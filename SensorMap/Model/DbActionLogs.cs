using System;

namespace SensorMap.Model
{
    public class DbActionLogs
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
