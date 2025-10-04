using Microsoft.EntityFrameworkCore;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
