using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.EntityFrameworkCore;
using NLog;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SensorMap.ViewModel
{
    public class SensorVM:ReactiveObject
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly INavigation _navigation;
        private readonly IDataService _service;
        private readonly IJsonSerialization _json;
        private readonly ITempImage imgManag;
        private IAppDbContextFactory _appDbContextFactory;
        private readonly IFileManagment _fileManagment;
        private Sensor _sensorsTreeNode;
        public ObservableCollection<AdditionalData> _additionalData;
        private List<Mechanism> Mechanisms { get; set; }
        private ObservableCollection<Mechanism> _FilteredMechanisms;
        private ObservableCollection<SensorCharacteristic> _sensorCharacteristics;

        private bool isEditMode;

        [Reactive]public Sensor SelectedNode
        {
            get => _sensorsTreeNode;
            set 
            {
                if(value!=null)
                this.RaiseAndSetIfChanged(ref _sensorsTreeNode, value);
            }
            
        }
        [Reactive]public ObservableCollection<Sensor> Sensors {  get; set; }
        
        [Reactive] public ObservableCollection<Mechanism> FilteredMechanisms 
        {
            get => _FilteredMechanisms;
            set { this.RaiseAndSetIfChanged(ref _FilteredMechanisms, value); }
        }
        [Reactive] public TreeViewCollection<SensorType, Sensor> SensorsTree { get; set; }
        private List<SensorType> sensorTypes {  get; set; }
        
        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }

        public SensorVM(IDataService service, IJsonSerialization json,
            ITempImage imgManag,
            INavigation navigation, IAppDbContextFactory appDbContextFactory,
            IFileManagment fileManagment, Sensor sensor = null)
        {
            SelectedNode = sensor;
            _navigation = navigation;
            _json = json;
            this.imgManag = imgManag;
            _service = service;
            _appDbContextFactory = appDbContextFactory;
            _fileManagment = fileManagment;
            using (var _dbContext = _appDbContextFactory.CreateDbContext())
            {
                // Загружаем датчики вместе с их типами и характеристиками одним пакетом
                var sensorsWithDetails = _dbContext.Sensors
                    .AsNoTracking()
                    .Include(s => s.Files)
                    .Include(s => s.SensorType)
                        .ThenInclude(st => st.Characteristics)
                    .ToList();

                Sensors = new(sensorsWithDetails);

                // Извлекаем уникальные типы датчиков из уже полученной коллекции
                sensorTypes = new(sensorsWithDetails
                    .Select(s => s.SensorType!)
                    .Where(st => st != null)
                    .DistinctBy(st => st.Id)
                    .ToList());
                Mechanisms = _dbContext.Mechanisms.AsNoTracking().Select(x=> new Mechanism 
                    {
                        Id=x.Id,
                        Name=x.Name,
                        SectorID = x.SectorID,
                        MapObjects = new(x.MapObjects.OfType<SensorAssignments>().Select(x => new SensorAssignments { SensorId = x.SensorId }).ToList())

                    }).ToList();
                Func<SensorType, Sensor, bool> filter = (type, sensor) => sensor.SensorTypeID == type.Id;
                SensorsTree = new TreeViewCollection<SensorType, Sensor>("Name", new(sensorTypes), Sensors, filter);
            }
            _additionalData = new(LoadMoreData());

            SaveMoreData = new RelayCommand<Sensor>((_)=>SaveDataFileds(),
                (_node) => { if (_node == null || _node.AdditionalData==null) return false;
                        return _node.AdditionalData.HasData() && IsEditMode; 
                });
            NavigateToMech = new RelayCommand<Mechanism>((mech) =>
            {
                if (mech == null) return;
                _navigation.NavigateTo<MechanismVM>(mech);
            });
            AddFiles = new RelayCommand<Sensor>((s) => { fileManagment.AddHelpfulFile(imgManag, s,true);  });
            DeletePathFiles = new RelayCommand<HelpfulFile>((file) =>
            {
                
                try
                {
                    using (var _dbContext = _appDbContextFactory.CreateDbContext())
                    {
                        var fileInDb = _dbContext.HelpfulFiles.Find(file!.Id);

                        if (fileInDb != null)
                        {
                            _dbContext.HelpfulFiles.Remove(fileInDb);
                            _dbContext.SaveChanges();
                        }
                        SelectedNode.Files.Remove(file);
                    }
                        Growl.Success(new GrowlInfo
                        {
                            Message = "Путь к файлам удален.",
                            CancelStr = "Ignore",
                            ShowDateTime = false,
                            WaitTime = 2
                        });
                }
                catch (Exception ex)
                {
                    Growl.Error("Ошибка при удаление путей!");
                    Logger.Error(ex.Message);
                }
            }, (file) => { return file != null; });
            OpenFile = new RelayCommand<HelpfulFile>((file) => 
            { 
                if (!fileManagment.OpenFileInExplorer(file.NameFile))
                {
                    MessageBoxResult res = HandyControl.Controls.MessageBox.Show(
                        new MessageBoxInfo
                        {
                            Message = "Файл не найден, скрыть файл?",
                            Caption = "Ошибка",
                            DefaultResult = MessageBoxResult.No
                        });
                    if (res == MessageBoxResult.OK)
                    {
                        file.IsHide = true;
                        
                    }
                } 
            });
            SaveFiles = new RelayCommand(() =>
            {
                try
                {
                    using (var _dbContext = _appDbContextFactory.CreateDbContext())
                    {
                        _dbContext.Attach(SelectedNode);

                        // Говорим EF Core, что навигационное свойство Files было изменено целиком
                        _dbContext.Entry(SelectedNode).Collection(s => s.Files).IsModified = true;
                        bool success = _dbContext.SaveChanges() > 0 ? true:false;
                        if (success) UnSetIsNew(SelectedNode.Files);
                            
                    }
                        Growl.Success(new GrowlInfo
                        {
                            Message = "Путь к файлам сохранен.",
                            CancelStr = "Ignore",
                            ShowDateTime = false,
                            WaitTime = 2
                        });
                }
                catch (Exception ex)
                {
                    Growl.Error("Ошибка при сохранение путей!");
                    Logger.Error(ex.Message);
                }
                
            });
            ShowAllFiles = new RelayCommand(() => 
            {
                foreach (var item in SelectedNode.Files)
                {
                    item.IsHide = false;
                }
            });
            OpenFullScreen = new RelayCommand<byte[]>((image) => 
            {
                imgManag.OpenFullScreen(imgManag.CreateImageFromBytes(image!)); 
            }, (image) => { return image != null; });

            this.WhenAnyValue(x => x.SelectedNode)
                .Where(sensor => sensor != null)
                .Select(sensor => Mechanisms.Where(mech =>mech.MapObjects!=null)
                .Where(x=>x.MapObjects!.OfType<SensorAssignments>().Any(sa => sa.SensorId == sensor.Id)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(filteredMechanisms =>
                { 
                    FilteredMechanisms = new(filteredMechanisms); 
                });
            
            _service.WhenAnyValue(x => x.IsEditMode)
                .BindTo(this, x => x.IsEditMode);
        }

        private List<AdditionalData> LoadMoreData()
        {
            List<AdditionalData> tempList = new List<AdditionalData>();
            if (File.Exists("SensorMoreData.json"))
            {
                //заполнение данными из файла
                foreach (var item in _json.ReadFromJsonFile<List<AdditionalData>>("SensorMoreData.json"))
                {
                    var sensor = Sensors.FirstOrDefault(x => x.Name == item.Name);
                    if (sensor != null)
                    {
                        if (item.HasData())
                        {
                            sensor.AdditionalData = item;
                        }
                        tempList.Add(sensor.AdditionalData);
                    }
                }
            }

            foreach (var sensorType in sensorTypes.Where(s => s.Characteristics.Any()))
            {
                var SensorsOneType = Sensors.Where(x => x.SensorTypeID == sensorType.Id);
                foreach (var sensor in SensorsOneType)
                {
                    if (sensor != null && !tempList.Contains(sensor.AdditionalData))
                    {
                        sensor.AdditionalData = AdditionalData.CreateRecord(sensor.Name, sensorType.Characteristics!);
                        tempList.Add(sensor.AdditionalData);
                    }
                    if (tempList.Contains(sensor.AdditionalData))
                    {
                        if(sensor.AdditionalData == null)
                        {
                            sensor.AdditionalData = AdditionalData.CreateRecord(sensor.Name, sensorType.Characteristics!);
                            break;
                        }
                        sensor.AdditionalData.Data = new (sensorType.Characteristics!
                            .Select(c => new MoreData
                            {
                                Parameter = c.Title, 
                                Value = sensor.AdditionalData.Data
                                .FirstOrDefault(d => d.Parameter == c.Title)?.Value ?? string.Empty 
                            }).ToList());
                    }
                }
               
            }
            return tempList;
        }

        public ICommand SaveMoreData { get; }
        public ICommand AddFiles { get; }
        public ICommand DeletePathFiles { get; }
        public ICommand SaveFiles { get; }
        public ICommand ShowAllFiles { get; }
        public ICommand OpenFile { get; }
        public ICommand NavigateToMech {  get; }
        public ICommand OpenFullScreen { get; }

        private void SaveDataFileds()
        {
            string FILE_PATH = "SensorMoreData.json";
            try
            {
                SelectedNode.AdditionalData.Name = SelectedNode.Name;
                var editableObject = _additionalData.Where(x => x.Name == SelectedNode.Name).FirstOrDefault();
                if (editableObject != null)
                {
                    editableObject.Data = SelectedNode.AdditionalData.Data;
                }
                else
                {
                    _additionalData.Add(SelectedNode.AdditionalData);
                }
                _json.WriteToJsonFile<ObservableCollection<AdditionalData>>(FILE_PATH, _additionalData);
                Growl.Success(new GrowlInfo
                {
                    Message = "Дополнительные данные сохранены!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
            catch
            {
                Growl.Error(new GrowlInfo
                {
                    Message = "Не удалось сохранить данные!",
                    CancelStr = "Ignore",
                    ShowDateTime = false,
                    WaitTime = 2
                });
            }
        }
        private void UnSetIsNew(IEnumerable<HelpfulFile> files)
        {
            foreach (var item in files)
            {
                item.IsNew = false;
            }
        }
    }
}
