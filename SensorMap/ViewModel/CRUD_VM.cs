using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class CRUD_VM:ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        private readonly IDataService _service;
        [Reactive] public INavigation Navigation { get; set; }
        [Reactive] public ObservableCollection<Sector> Sectors { get; set; } = new();
        [Reactive] public ObservableCollection<Sensor> Sensors { get; set; } = new();
        [Reactive] public ObservableCollection<Mechanism> Mechanisms { get; set; } = new();
        public CRUD_VM(IDataBaseProvider provider,IDataService service,INavigation nav) 
        {
            Navigation = nav;
            _provider = provider;
            _service = service;
            Sectors = _service.Sectors;
            Sensors = _service.Sensors;
            Mechanisms = _service.Mechanisms;
            ShowCommand =new RelayCommand<object>((Sensor)=> 
            {
                if(Sensor is Sensor) 
                    Navigation.ShowDialog<SensorView, SensorVM>(Sensor); 
            });
            SaveCommand = new RelayCommand<object>((arg) => 
            {
                switch (arg)
                {
                    case Sector sector:
                        {
                            if (sector.Id == 0) _provider.Create<Sector>(sector);
                            else _provider.Update<Sector>(sector);
                        } break;
                    case Sensor sensor:
                        {
                            if (sensor.Id == 0) _provider.Create<Sensor>(sensor);
                            else _provider.Update<Sensor>(sensor);
                        }
                        break;
                    case Mechanism mechanism:
                        {
                            if (mechanism.Id == 0) _provider.Create<Mechanism>(mechanism);
                            else _provider.Update<Mechanism>(mechanism);
                        }
                        break;
                }
            });
            DeleteCommand = new RelayCommand<object>((arg) => 
            {
                switch (arg)
                {
                    case Sector sector:
                        {
                            _provider.Delete<Sector>(sector);
                            Sectors.Add(new Sector());
                        }
                        break;
                    case Sensor sensor:
                        {
                            _provider.Delete<Sensor>(sensor);
                            Sensors.Add(new Sensor());
                        }
                        break;
                    case Mechanism mechanism:
                        {
                            _provider.Delete<Mechanism>(mechanism);
                            Mechanisms.Add(new Mechanism());
                        }
                        break;
                }
            });
            CancelCommand = new RelayCommand<object>((arg)=>
            {
                switch (arg)
                {
                    //case Sector sector: Sectors.Remove(sector); break;
                    case Sensor sensor: sensor.CancelEdit(); break;
                    //case Mechanism mechanism: Mechanisms.Remove(mechanism); break;
                }
            });
            AddImage = new RelayCommand(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Файлы рисунков (*.bmp, *.jpg)|*.bmp;*.jpg;*.png;*.jpeg|Все файлы (*.*)|*.*";
                openFileDialog.ShowDialog();
            });
        }

        //Получить событие DG editable и от него появляется кнопка Save
        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand ShowCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand AddImage {  get; set; }

    }
}
