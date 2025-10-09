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
                if (arg is Sensor sensor)
                {
                    sensor.IsModified = false;
                    _provider.CreateSensor(sensor);
                }
            });
            DeleteCommand = new RelayCommand<object>((arg) => 
            {
                switch (arg)
                {
                    case Sector sector: Sectors.Remove(sector); break;
                    case Sensor sensor: Sensors.Remove(sensor); break;
                    case Mechanism mechanism: Mechanisms.Remove(mechanism); break;
                }

            });
            CancelCommand = new RelayCommand<object>((arg)=>
            {
               // GetObjectFromDBAsync(arg);
            });
            AddImage = new RelayCommand(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Файлы рисунков (*.bmp, *.jpg)|*.bmp;*.jpg;*.png;*.jpeg|Все файлы (*.*)|*.*";
                openFileDialog.ShowDialog();
            });
        }

        private async void GetObjectFromDBAsync(object? arg)
        {
            if (arg != null)
            {
                var entityType = arg.GetType();
                var idProperty = entityType.GetProperty("Id");

                if (idProperty != null)
                {
                    //var freshEntity = await _provider.GetElementByID<>((int)idProperty.GetValue(arg));
                    //if (freshEntity != null)
                    {
                        // Копируем значения свойств
                        //CopyProperties(freshEntity, arg);

                        // Сбрасываем флаг модификации
                        var isModifiedProperty = entityType.GetProperty("IsModified");
                        isModifiedProperty?.SetValue(arg, false);
                    }
                }
            }
        }

        private void CopyProperties(object source, object destination)
        {
            var properties = source.GetType().GetProperties()
        .Where(p => p.CanRead && p.CanWrite && p.Name != "IsModified");

            foreach (var property in properties)
            {
                var value = property.GetValue(source);
                property.SetValue(destination, value);
            }
        }

        //Получить событие DG editable и от него появляется кнопка Save
        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand ShowCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand AddImage {  get; set; }

    }
}
