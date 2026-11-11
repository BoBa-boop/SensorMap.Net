using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
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
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

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
            if (Mechanism != null && Mechanism.MapObjects != null)
                Mechanism.MapObjects.RemoveMany(Mechanism.MapObjects.Where(x => x.ToDelete == true).ToList());
            SensorList = new (sensorsList);
            DeviceList = new(deviceList);
            Sensors = Mechanism.MapObjects.OfType<SensorAssignments>().ToList();
            Devices = Mechanism.MapObjects.OfType<DeviceAssignment>().ToList();
            
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
            DoCurrentAddress = new RelayCommand(() =>
            {
                if (Sensors.Count <= 0) return;
                CultureInfo cultureInfo = CultureInfo.CurrentCulture;
                char separator = cultureInfo.NumberFormat.NumberDecimalSeparator[0];
                var pattern = $@"^\d+\{separator}\d+$";
                foreach (var sensor in Sensors)
                {
                    if (!Regex.IsMatch(sensor.Address, pattern))
                    {
                        // 1. Оставляем только цифры и разделители (точка/запятая)
                        string cleaned = Regex.Replace(sensor.Address, @"[^\d.,]", "");

                        // 2. Нормализуем разделитель: запятую меняем на точку
                        cleaned = cleaned.Replace(',', '.');

                        
                        // 4. Если получилось что-то валидное — приводим к формату y.x
                        if (Regex.IsMatch(cleaned, @"^\d+\.\d+$"))
                        {
                            sensor.Address = cleaned;
                        }
                        else if (Regex.IsMatch(cleaned, @"^\d+$"))
                        {
                            // только целая часть — добавляем ".0" (или оставляем как есть, по вашему требованию)
                            sensor.Address = cleaned + ".0";
                        }
                        else if (Regex.IsMatch(cleaned, @"^\.\d+$"))
                        {
                            // только дробная часть — добавляем "0" в начало
                            sensor.Address = "0" + cleaned;
                        }
                        if (Regex.IsMatch(sensor.Address, pattern))
                        {
                            sensor.IsModified = true;
                        }
                    }
                }
                if (Sensors.Where(x => x.IsModified).Any()) HasChanges=true;
            }, ()=> { return Sensors.Count > 0; });
            this.WhenAnyValue(x => x.SelectedMapObject)
            .Select(obj =>
            {
                if (obj == null) return Observable.Empty<string>();

                //var common = Observable.Merge(
                //    obj.WhenAnyValue(m => m.Description).Skip(1).Select(_ => "Description"),
                //    obj.WhenAnyValue(m=>m.Image).Skip(1).Select(_=>"Image"));
                //return obj switch
                //{
                //    SensorAssignments s => common.Merge(
                //        s.WhenAnyValue(x => x.Address).Skip(1).Select(_ => "Address"),
                //        s.WhenAnyValue(x => x.Sensor.Name).Skip(1).Select(_ => "Sensor.Name")),
                //    DeviceAssignment d => common.Merge(
                //        d.WhenAnyValue(x => x.Device.Name).Skip(1).Select(_ => "Device.Name")),
                //    _ => common
                //};

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
        public ICommand DoCurrentAddress { get; set; }
    }
}
