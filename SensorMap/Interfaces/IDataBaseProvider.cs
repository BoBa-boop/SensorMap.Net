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
        IReadOnlyList<IDbCheck> CreateChecks();
        bool ChangeDataBase(string path);
        Task CreateBackupDB(string backupDir);
    }
}
