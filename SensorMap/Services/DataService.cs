using SensorMap.Interfaces;

namespace SensorMap.Services
{
    public class DataService : IDataService
    {

        public bool IsEditMode { get; set; }
        public DataService()
        {
            IsEditMode = false;
        }
        public void Add()
        {
            throw new NotImplementedException();
        }
    }
}
