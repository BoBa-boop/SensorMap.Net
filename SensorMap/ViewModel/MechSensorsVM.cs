using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Behaviors;
using SensorMap.CustomControls;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class MechSensorsVM:ReactiveObject
    {
        private Mechanism _mechanism;
        private MapObject _selectedMapObject;
        private ITempImage _imageControl;
        private bool isEditMode;
        private bool _hasChanges;

        private List<SensorAssignments> _sensors;
        private List<DeviceAssignment> _devices;

        [Reactive]public List<SensorAssignments> Sensors
        {
            get { return _sensors; }
            set { this.RaiseAndSetIfChanged(ref _sensors, value); }
        }
        [Reactive]
        public List<DeviceAssignment> Devices
        {
            get { return _devices; }
            set { this.RaiseAndSetIfChanged(ref _devices, value); }
        }
        [Reactive]public Mechanism Mechanism
        {
            get { return _mechanism; }
            set { _mechanism = value; this.RaiseAndSetIfChanged(ref _mechanism, value); }
        }

        /// <summary>
        /// Выбранный объект на карте (датчик или устройство)
        /// </summary>
        [Reactive]
        public MapObject SelectedMapObject
        {
            get { return _selectedMapObject; }
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedMapObject, value);
            }
        }
        

        [Reactive]
        public bool HasChanges
        {
            get { return _hasChanges; }
            set
            {
                 _hasChanges = value;
                this.RaiseAndSetIfChanged(ref _hasChanges, value);
            }
        }
        [Reactive] public ObservableCollection<Sensor> SensorList{get;set;}
        [Reactive] public ObservableCollection<Device> DeviceList { get; set; }
        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }
        public MechSensorsVM(ITempImage imageControl,Mechanism currentMech,IEnumerable<Sensor> sensorsList, IEnumerable<Device> deviceList)
        {
            Mechanism = (Mechanism)currentMech.Clone();
            SensorList = new (sensorsList);
            DeviceList = new(deviceList);
            Sensors = Mechanism.MapObjects.OfType<SensorAssignments>().ToList();
            Devices = Mechanism.MapObjects.OfType<DeviceAssignment>().ToList();
            if (Mechanism!=null && Mechanism.MapObjects!=null)
                Mechanism.MapObjects.RemoveMany(Mechanism.MapObjects.Where(x => x.ToDelete == true).ToList());
            _imageControl = imageControl;
            AddImage = new RelayCommand<MapObject>((obj) =>
            {
                byte[] tempImage = _imageControl.OpenImageDialog();
                if (!tempImage.IsNullOrEmpty())
                {
                    SelectedMapObject.Image = tempImage;
                    SelectedMapObject.IsModified = true;
                    HasChanges = true;
                }
                
            });
            ShowPreviewImage = new RelayCommand<object>((image) =>
            {
                if (image is byte[] img && img != null)
                {
                    var browser = new CustomImageBrowser(_imageControl.CreateImageFromBytes(img)) { Title = "Просмотр" };
                    browser.ShowDialog();
                }
            });
            this.WhenAnyValue(x => x.SelectedMapObject)
            .Select(obj =>
            {
                if (obj == null) return Observable.Empty<string>();

                if (obj == null) return Observable.Empty<string>();

                // Свойства самого объекта (пропускаем первое значение сразу)
                var descriptionChanged = obj.WhenAnyValue(m => m.Description).Skip(1).Select(_ => "Description");
                var imageChanged = obj.WhenAnyValue(m => m.Image).Skip(1).Select(_ => "Image");

                // Ищем связанные элементы. Используем метод для безопасного получения потока.
                var addressChanged = GetSensorObservable(obj.Id, s => s.WhenAnyValue(x => x.Address),"Address");
                var nameSensorChanged = GetSensorObservable(obj.Id, s => s.WhenAnyValue(x => x.Sensor.Name),"Sensor.Name");
                var nameDeviceChanged = GetDeviceObservable(obj.Id, d => d.WhenAnyValue(x => x.Device.Name),"Device.Name");

                return Observable.Merge(
                    descriptionChanged,
                    imageChanged,
                    addressChanged,
                    nameSensorChanged,
                    nameDeviceChanged
                );
            })
            .Switch()
            .Subscribe(propertyName =>
            {
                SelectedMapObject.IsModified = true;
                HasChanges = true;
            });
        }
        private IObservable<string> GetSensorObservable<TProperty>(int objId, Func<SensorAssignments, IObservable<TProperty>> propertySelector, string propertyName)
        {
            // Находим датчик для текущего ID
            var sensor = Sensors.FirstOrDefault(x => x.Id == objId);

            if (sensor == null)
                return Observable.Empty<string>(); // Если датчика нет, просто возвращаем пустой поток

            // Берем свойство, пропускаем стартовое значение и трансформируем в строку-маркер
            return propertySelector(sensor)
                .Skip(1)
                .Select(_ => propertyName);
        }

        private IObservable<string> GetDeviceObservable<TProperty>(int objId, Func<DeviceAssignment, IObservable<TProperty>> propertySelector, string propertyName)
        {
            var device = Devices.FirstOrDefault(x => x.Id == objId);

            if (device == null)
                return Observable.Empty<string>();

            return propertySelector(device)
                .Skip(1)
                .Select(_ => propertyName);
        }
        public ICommand AddImage { get; set; }
        public ICommand ShowPreviewImage { get; set; }
    }
}
