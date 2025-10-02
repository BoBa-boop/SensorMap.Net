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
        private readonly AppDBContext _context;

        public bool IsEditMode { get; set; }
        public bool IsReadMode { get; set; }
        public DataService(AppDBContext context)
        {
            _context = context;
            IsEditMode = false;
            IsReadMode = !IsEditMode;
        }
        public void Add()
        {
            throw new NotImplementedException();
        }

        public async Task<ObservableCollection<Sector>> GetSectorsAsync()
        {
            var sectors = await _context.Sectors
            .Include(s => s.Mechanisms)
                .ThenInclude(m => m.Sensors)
                    .ThenInclude(sa => sa.Sensor)
            .Include(s => s.Mechanisms)
                .ThenInclude(m => m.PLC)
            .AsNoTracking()
            .ToListAsync();

            return new ObservableCollection<Sector>(sectors);
        }
    }
}
