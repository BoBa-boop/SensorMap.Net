using CommunityToolkit.Mvvm.Input;
using DynamicData;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Behaviors;
using SensorMap.Converters;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SensorMap.ViewModel
{
    public class CRUD_VM:ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        private readonly IJsonSerialization _json;
        private readonly IDataService _service; 
        private readonly ITempImage _tempImage;
        private IAppDbContextFactory _appDbContextFactory;
        private AppDBContext _dBContext;
        private bool isEditMode;

        public readonly UndoRedoStack _undoRedoManager = new UndoRedoStack();
        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }
        [Reactive] public bool CanUndo => _undoRedoManager.CanUndo;
        [Reactive] public bool CanRedo=>_undoRedoManager.CanRedo;
        [Reactive] public INavigation Navigation { get; set; }
        [Reactive] public ObservableCollection<Sector> Sectors { get; set; }
        [Reactive] public ObservableCollection<SensorAssignments> SensorAssignments { get; set; }
        [Reactive] public ObservableCollection<Sensor> Sensors { get; set; }
        [Reactive] public ICollectionView Devices { get; set; }
        [Reactive] public ObservableCollection<SensorType> SensorTypes { get; set; }
        [Reactive] public ObservableCollection<DeviceType> DeviceTypes { get; set; }
        [Reactive] public ObservableCollection<string> Manufacturers { get; set; }
        [Reactive] public ICollectionView Mechanisms {  get; set; }

        public CRUD_VM(IDataBaseProvider provider, IDataService service, IAppDbContextFactory cxFactory,
            IJsonSerialization json,INavigation nav, ITempImage tempImage)
        {
            Navigation = nav;
            _tempImage = tempImage;
            _json = json;
            _provider = provider;
            _appDbContextFactory = cxFactory;
            _service = service;
            using (var dBContext = _appDbContextFactory.CreateDbContext())
            {
                Sectors = new(dBContext.Sectors.ToList());
                Mechanisms = CollectionViewSource.GetDefaultView(dBContext.Mechanisms.ToList());
                SensorAssignments = new(dBContext.SensorAssignments.ToList());
                Devices = CollectionViewSource.GetDefaultView(dBContext.Devices.ToList());
                Manufacturers = new(Devices.OfType<Device>().Where(x=>!string.IsNullOrEmpty(x.Manufacturer)).Select(device => device.Manufacturer).Distinct().ToList());
                Sensors = new(dBContext.Sensors.ToList());
                SensorTypes = new(dBContext.SensorTypes.Include(x => x.Characteristics).ToList());
                DeviceTypes = new(dBContext.DeviceTypes.Include(x => x.Characteristics).ToList());
            }
            using (Mechanisms.DeferRefresh())
            {
                Mechanisms.SortDescriptions.Add(new SortDescription("Sector.Name", ListSortDirection.Ascending));
                Mechanisms.GroupDescriptions.Add(new PropertyGroupDescription("Sector.Name"));
                var groupDescription = new PropertyGroupDescription("Name", new EqualMechGroup());
                Mechanisms.GroupDescriptions.Add(groupDescription);
            }
            using (Devices.DeferRefresh())
            {
                Devices.SortDescriptions.Add(new SortDescription("DeviceType.Name", ListSortDirection.Ascending));
                Devices.GroupDescriptions.Add(new PropertyGroupDescription("DeviceType.Name"));
            }

            ShowCommand = new RelayCommand<object>((Sensor) =>
            {
                if (Sensor is Sensor)
                    Navigation.NavigateTo<SensorVM>(Sensor);
            });
            SaveCommand = new RelayCommand<object>((arg) =>
            {
                if (arg is null) return;
                using (var dBContext = _appDbContextFactory.CreateDbContext())
                {
                    try
                    {
                        dBContext.Update(arg);
                        dBContext.SaveChanges();
                        arg.GetType()?.GetProperty("IsModified")?.SetValue(arg, false);
                        Growl.Success(new GrowlInfo
                        {
                            Message = "Данные в БД изменены",
                            CancelStr = "Ignore",
                            ShowDateTime = false,
                            WaitTime = 2
                        });
                    }
                    catch
                    {
                        Growl.Error("Ошибка при изменение БД");
                    }
                }

            });
            DeleteCommand = new RelayCommand<object>((arg) =>
            {
                if (arg is null) return;
                var entityType = arg.GetType();
                try
                {
                    using (var dBContext = _appDbContextFactory.CreateDbContext())
                    {
                        var result = System.Windows.MessageBox.Show("Вы действительно хотите удалить строчку?", "Подтверждение",
                                MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            if (dBContext.Entry(arg).IsKeySet)
                            {
                                dBContext.Remove(arg);
                                dBContext.SaveChanges();
                                Growl.Success(new GrowlInfo
                                {
                                    Message = "Данные удалены!",
                                    CancelStr = "Ignore",
                                    ShowDateTime = false,
                                    WaitTime = 2
                                });
                            }
                            PropertyInfo? prop = typeof(CRUD_VM).GetProperty(entityType.Name + "s");
                            if (prop != null)
                            {
                                var collection = prop.GetValue(this) as System.Collections.IList;
                                collection?.Remove(arg);
                            }
                        }
                    }
                }
                catch
                {
                    Growl.Error(new GrowlInfo
                    {
                        Message = "Ошибка при удаление!",
                        CancelStr = "Ignore",
                        ShowDateTime = false,
                        WaitTime = 2
                    });
                }


            });

            AddImage = new RelayCommand<object>((arg) =>
            {
                if (arg is null) return;
                var entityType = arg.GetType();
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Файлы рисунков (*.bmp, *.jpg, *.png, *.jpeg)|*.bmp;*.jpg;*.png;*.jpeg|Все файлы (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    // Прочитаем выбранный файл в массив байтов
                    using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        byte[] photoBytes = new byte[fs.Length];
                        fs.ReadExactly(photoBytes);
                        entityType.GetProperty("Image")?.SetValue(arg, photoBytes);
                    }
                }
            });
            ShowPreviewImage = new RelayCommand<object>((image) =>
            {
                if (image is byte[] img && img != null)
                {
                    var browser = new CustomImageBrowser(_tempImage.CreateImageFromBytes(img)) { Title = "Просмотр схемы" };
                    browser.ShowDialog();
                }
            });
            AddSensorType = new RelayCommand<object>((param) =>
            {
                if (param is null) return;

                var values = (object[])param;
                var name = (string)values[0];
                var image = (Uri)values[1];
                byte[] photoBytes;
                SensorType sType = new SensorType();
                sType.Name = name;

                if (image != null)
                {
                    using (FileStream fs = new FileStream(image.LocalPath, FileMode.Open, FileAccess.Read))
                    {
                        photoBytes = new byte[fs.Length];
                        fs.ReadExactly(photoBytes);
                    }
                    sType.Image = photoBytes;
                }
                if (!SensorTypes.Where(x => x.Name == sType.Name).Any())
                {
                    sType.IsNew = true;
                    SensorTypes.Add(sType);
                }
            }, (param) =>
            {
                if (param == null) return false;
                var values = (object[])param;
                return !string.IsNullOrWhiteSpace(values[0].ToString());
            });
            AddDeviceType = new RelayCommand<object>((param) =>
            {
                if (param is null) return;

                var values = (object[])param;
                var name = (string)values[0];
                var image = (Uri)values[1];
                byte[] photoBytes;
                DeviceType sType = new DeviceType();
                sType.Name = name;

                if (image != null)
                {
                    using (FileStream fs = new FileStream(image.LocalPath, FileMode.Open, FileAccess.Read))
                    {
                        photoBytes = new byte[fs.Length];
                        fs.ReadExactly(photoBytes);
                    }
                    sType.Image = photoBytes;
                }
                if (!DeviceTypes.Where(x => x.Name == sType.Name).Any())
                {
                    sType.IsNew = true;
                    DeviceTypes.Add(sType);
                }
            }, (param) =>
            {
                if (param == null) return false;
                var values = (object[])param;
                return !string.IsNullOrWhiteSpace(values[0].ToString());
            });
            DeleteNodeTitleType = new RelayCommand<object>((type) =>
            {
                if (type is SensorType sensorType && sensorType != null)
                {
                    var res = System.Windows.MessageBox.Show("Удаление типа, также включает УДАЛЕНИЕ датчиков и параметров этого типа! Подтвердите действие.",
                        "Подтверждение действий", MessageBoxButton.OKCancel,MessageBoxImage.Warning);
                    if(res == MessageBoxResult.OK) 
                    {
                        SensorTypes.Remove(sensorType);
                        using (var dBContext = _appDbContextFactory.CreateDbContext())
                        {
                            if (dBContext.SensorTypes.Contains(sensorType))
                            {
                                dBContext.SensorTypes.Remove(sensorType);
                                dBContext.SaveChanges();
                            }
                        }
                    }
                }
                if (type is DeviceType deviceType && deviceType != null)
                {
                    var res = System.Windows.MessageBox.Show("Удаление типа, также включает УДАЛЕНИЕ устройств и параметров этого типа! Подтвердите действие.",
                        "Подтверждение действий", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.OK)
                    {
                        DeviceTypes.Remove(deviceType);
                        using (var dBContext = _appDbContextFactory.CreateDbContext())
                        {
                            if (dBContext.DeviceTypes.Contains(deviceType))
                            {
                                dBContext.DeviceTypes.Remove(deviceType);
                                dBContext.SaveChanges();
                            }
                        }
                    }
                }
            },
            (type) => { return type != null; });

            AddCharacteristic = new RelayCommand<object>((type) =>
            {
                if (type is SensorType sensorType)
                    sensorType.Characteristics.Add(new SensorCharacteristic() { Title = "Новая характеристика",SensorTypeId=sensorType.Id });
                if(type is DeviceType deviceType)
                    deviceType.Characteristics.Add(new DeviceCharacteristic() { Title = "Новая характеристика", DeviceTypeId = deviceType.Id });
            }, (type) => { return type != null; });
            DeleteCharacteristic = new RelayCommand<object>((type) =>
            {
                if (type is SensorCharacteristic sensorCharact && sensorCharact!=null)
                {
                    var res = System.Windows.MessageBox.Show("Операция включает в себя удаление записанных данных в выбранный параметр.\r" +
                        $"Датчики имеющий тип {sensorCharact.Title} также утратят параметр и данные! Подтвердите действие.",
                        "Подтверждение действий", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.OK)
                    {
                        using (var dBContext = _appDbContextFactory.CreateDbContext())
                        {
                            
                            dBContext.SensorCharacteristic.Remove(sensorCharact);
                            dBContext.SaveChanges();
                        }
                        DeleteFromFile(sensorCharact);
                    }
                    
                }
                if (type is DeviceCharacteristic deviceCharacteristic && deviceCharacteristic != null)
                {
                    var res = System.Windows.MessageBox.Show("Операция включает в себя удаление записанных данных в выбранный параметр.\r" +
                        $"Датчики имеющий тип {deviceCharacteristic.Title} также утратят параметр и данные! Подтвердите действие.",
                        "Подтверждение действий", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.OK)
                    {
                        using (var dBContext = _appDbContextFactory.CreateDbContext())
                        {

                            dBContext.DeviceCharacteristic.Remove(deviceCharacteristic);
                            dBContext.SaveChanges();
                        }
                        DeleteFromFile(deviceCharacteristic);
                    }

                }
            },(type) => { return type != null; });

            UndoCommand = new RelayCommand(_undoRedoManager!.Undo);
            RedoCommand = new RelayCommand(_undoRedoManager.Redo);

            _service.WhenAnyValue(x => x.IsEditMode)
                .BindTo(this, x => x.IsEditMode);

            _undoRedoManager.WhenAnyValue(x => x.CanUndo)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(CanUndo)));

            _undoRedoManager.WhenAnyValue(x => x.CanRedo)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(CanRedo)));
        }

        private void DeleteFromFile(object characteristic)
        {
            if(characteristic is SensorCharacteristic sensorCharacteristic)
            {
                if (File.Exists("SensorMoreData.json"))
                {
                    var file = _json.ReadFromJsonFile<List<AdditionalData>>("SensorMoreData.json");
                    foreach (var item in file)
                    {
                        item.Data.RemoveMany(item.Data.Where(x => x.Parameter == sensorCharacteristic.Title));
                    }
                    _json.WriteToJsonFile("SensorMoreData.json", file);
                }
            }
            if (characteristic is DeviceCharacteristic deviceCharacteristic)
            {
                if (File.Exists("DeviceMoreData.json"))
                {
                    var file = _json.ReadFromJsonFile<List<AdditionalData>>("DeviceMoreData.json");
                    foreach (var item in file)
                    {
                        item.Data.RemoveMany(item.Data.Where(x => x.Parameter == deviceCharacteristic.Title));
                    }
                    _json.WriteToJsonFile("DeviceMoreData.json", file);
                }
            }
        }

        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand ShowCommand { get; set; }
        public ICommand AddImage {  get; set; }
        public ICommand ShowPreviewImage { get; set; }
        public ICommand AddSensorType { get; }
        public ICommand AddDeviceType { get; }
        public ICommand DeleteNodeTitleType { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand AddCharacteristic { get; set; }
        public ICommand DeleteCharacteristic { get; set; }
        public void RecordEdit<T>(T _inputObject, string propertyName, object oldValue, object newValue)
        {
            var command = new Commands.DataGridCommands.EditCell<T>(_inputObject, propertyName, oldValue, newValue);
            _undoRedoManager.Do(command);
        }
        
    }
}
