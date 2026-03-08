using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SensorMap.Services
{
    public class DataService :ReactiveObject, IDataService
    {
        private bool _isEdit;
        private Mechanism _curMech;
        private Sector _curSector;
        private bool _isConnect;
        private IAppDbContextFactory _appDbContextFactory;

        [Reactive]
        public bool IsEditMode
        {
            get => _isEdit;
            set => this.RaiseAndSetIfChanged(ref _isEdit, value);
        }
        [Reactive]
        public bool IsDataBaseConnect
        {
            get => _isConnect;
            set => this.RaiseAndSetIfChanged(ref _isConnect, value);
        }
        public Mechanism CurrentMechanism_Global 
        {
            get => _curMech;
            set => this.RaiseAndSetIfChanged(ref _curMech, value);
        }
        public Sector CurrentSector_Global 
        {
            get => _curSector;
            set => this.RaiseAndSetIfChanged(ref _curSector, value);
        }
        public ObservableCollection<Sensor> SensorDTO => GetSensorDTO();
        public ObservableCollection<SensorType> SensorTypeDTO => GetSensorTypeDTO();

        ObservableCollection<Sensor> IDataService.SensorDTO { get => SensorDTO; set => throw new NotImplementedException(); }
        ObservableCollection<SensorType> IDataService.SensorTypeDTO { get => SensorTypeDTO; set => throw new NotImplementedException(); }

        public DataService(IAppDbContextFactory appDbContextFactory)
        {
            _appDbContextFactory = appDbContextFactory;
        }

        private ObservableCollection<Sensor> GetSensorDTO()
        {
            using(var dbContext = _appDbContextFactory.CreateDbContext())
            {
                return new(dbContext.Sensors.AsNoTracking().Include(x=>x.SensorType).ToList());
            }
        }

        private ObservableCollection<SensorType> GetSensorTypeDTO()
        {
            using (var dbContext = _appDbContextFactory.CreateDbContext())
            {
                return new(dbContext.SensorTypes.AsNoTracking().ToList());
            }
        }
    }
}
