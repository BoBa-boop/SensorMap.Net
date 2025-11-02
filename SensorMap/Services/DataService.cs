using Microsoft.EntityFrameworkCore;
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
        private ObservableCollection<SensorType> _sensorTypes = new();
        private ObservableCollection<Mechanism> _mechanisms= new();
        private IDataBaseProvider _provider;


        private bool _isEdit;
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

        public DataService(IDataBaseProvider provider)
        {
            _provider = provider;
            GetDataFromDB();
        }

        private async void GetDataFromDB()
        {
            await Task.Run(async () => 
            {
                _sectors = new ObservableCollection<Sector>(await _provider.GetAllSectorsAsync());
                _sensors = new ObservableCollection<Sensor>(await _provider.GetAllSensors());
                _mechanisms = new ObservableCollection<Mechanism>(await _provider.GetAllMechanisms());
                _sensorTypes = new ObservableCollection<SensorType>(await _provider.GetSensortypeAsync());
            });
        }
    }
}
