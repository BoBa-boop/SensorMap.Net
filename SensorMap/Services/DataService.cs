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
        private ObservableCollection<Sensor> _sensors = new();
        private ObservableCollection<Sector> _sectors = new();
        private ObservableCollection<PLC> _plc = new();
        private ObservableCollection<SensorType> _sensorTypes = new();
        private ObservableCollection<Mechanism> _mechanisms= new();
        private ObservableCollection<string> _manufacturers = new();
        private IDataBaseProvider _provider;

        
        private bool _isEdit;
        private Mechanism _curMech;
        private Sector _curSector;

        [Reactive]
        public bool IsEditMode
        {
            get => _isEdit;
            set => this.RaiseAndSetIfChanged(ref _isEdit, value);
        }
        public ObservableCollection<Sensor> Sensors => _sensors;
        public ObservableCollection<Sector> Sectors => _sectors;
        public ObservableCollection<Mechanism> Mechanisms => _mechanisms;
        public ObservableCollection<SensorType> SensorTypes => _sensorTypes;
        public ObservableCollection<PLC> PLCs => _plc;
        public ObservableCollection<string> PLC_Manufacturers => _manufacturers;

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

        public DataService(IDataBaseProvider provider)
        {
            _provider = provider;
        }

        public async void UpdateDataFromDB()
        {
            await Task.Run(async () => 
            {
                _sectors = new(await _provider.GetAllSectorsAsync());
                _plc = new (await _provider.GetAllPLCsAsync());
                //_plc = new() { new PLC() { Name = "sd", Id = 15 } };
                _sensors = new (await _provider.GetAllSensors());
                _mechanisms = new (await _provider.GetAllMechanisms());
                _sensorTypes = new (await _provider.GetSensorTypeAsync());
            });
            //foreach (var plc in _plc)
            //{
            //    _manufacturers.Add(plc.Manufacturer);
            //}
        }
    }
}
