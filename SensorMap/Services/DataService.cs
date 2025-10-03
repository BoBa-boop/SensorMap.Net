using SensorMap.Interfaces;

namespace SensorMap.Services
{
    public class DataService : IDataService
    {

        public bool IsEditMode { get; set; }
        public bool IsReadMode { get; set; }
        public DataService()
        {
            IsEditMode = false;
            IsReadMode = !IsEditMode;
        }
        public void Add()
        {
            throw new NotImplementedException();
        }
    }
}
