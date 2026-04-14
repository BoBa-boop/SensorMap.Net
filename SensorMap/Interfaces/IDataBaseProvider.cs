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
        bool ChangeDataBase(string path);
        void CreateBackupDB(string backupDir);
    }
}
