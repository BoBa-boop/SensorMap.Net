using Microsoft.EntityFrameworkCore;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.EF
{
    public class UnitOfWork : IDisposable
    {
        private readonly AppDBContext _context;
        private bool disposed = false;

        public UnitOfWork(IAppDbContextFactory _factory)
        {
            _context = _factory.CreateDbContext();
        }

        public IRepository<Sector> Sectors => new SectorRepository(_context);
        public IRepository<Sensor> Sensors => new SensorRepository(_context);
        public IRepository<Mechanism> Mechanisms => new MechRepository(_context);
        public IRepository<SensorType> SensorTypes => new SensorTypeRepository(_context);
        public IRepository<PLC> PLCs => new PLC_Repository(_context);
        public async Task<int> Complete()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
                _context.Dispose();
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
