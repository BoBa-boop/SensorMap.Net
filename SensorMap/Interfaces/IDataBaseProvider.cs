using Microsoft.EntityFrameworkCore;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Interfaces
{
    public interface IDataBaseProvider
    {
        Task<IEnumerable<Sector>> GetAllSectorsAsync();
        Task<IEnumerable<Sensor>> GetAllSensors();
        Task<IEnumerable<PLC>> GetAllPLCsAsync();
        Task<IEnumerable<SensorType>> GetSensorTypeAsync();
        Task<T> GetElementByID<T>(int id) where T:class;
        Task<IEnumerable<Mechanism>> GetAllMechanisms();
        Task<IEnumerable<Mechanism>> GetAllMechanismsWithSector();
        Task AddSensorsAssignmentAsync(IEnumerable<SensorAssignments> coll);
        Task Create<T>(T entity) where T : class;
        Task Delete<T>(T entity) where T : class;
        Task Update<T>(T entity) where T : class;

        bool ChangeDataBase(string path);
        void CreateBackupDB(string backupDir,string name);
    }
}
