using SensorMap.Model;
using System.Collections.ObjectModel;

namespace SensorMap.Interfaces
{
    public interface ILogEntryService
    {
        ObservableCollection<DbActionLogs> Logs { get; }
    }
}
