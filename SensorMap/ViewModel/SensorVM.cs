using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SensorMap.ViewModel
{
    public class SensorVM : ReactiveObject, IActivatableViewModel
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
        private ObservableCollection<Mechanism> _FilteredMechanisms;
        private ObservableCollection<SensorCharacteristic> _sensorCharacteristics;

        private bool isEditMode;
        private TreeViewCollection<SensorType, Sensor> sensorsTree;
        private ObservableCollection<Sensor> sensors;

        [Reactive]public Sensor SelectedNode
        {
            get => _sensorsTreeNode;
            set 
            {
                if(value!=null)
                this.RaiseAndSetIfChanged(ref _sensorsTreeNode, value);
            }
            
        }
        [Reactive] public ObservableCollection<Sensor> Sensors 
        {
            get { return sensors; }
            set
            {
                this.RaiseAndSetIfChanged(ref sensors, value);
            }
        }

        [Reactive] public ObservableCollection<Mechanism> FilteredMechanisms 
        {
            get => _FilteredMechanisms;
            set { this.RaiseAndSetIfChanged(ref _FilteredMechanisms, value); }
        }
        [Reactive] public TreeViewCollection<SensorType, Sensor> SensorsTree 
        {
            get { return sensorsTree; }
            set
            {
                this.RaiseAndSetIfChanged(ref sensorsTree, value);
            }
        }
        private List<SensorType> sensorTypes {  get; set; }
        private Dictionary<string, AdditionalData> _jsonMoreDataCache = new();
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



            SaveMoreData = new RelayCommand<Sensor>((_) => SaveDataFileds(),
                (_node) =>
                {
                    if (_node == null || _node.AdditionalData == null) return false;
                    return _node.AdditionalData.HasData() && IsEditMode;
                });
            NavigateToMech = new RelayCommand<Mechanism>((mech) =>
            {
                if (mech == null) return;
                _navigation.NavigateTo<MechanismVM>(mech);
            });
            AddFiles = new RelayCommand<Sensor>((s) =>
            {
                string[] paths = fileManagment.OpenFileDialog(true);
                fileManagment.AddHelpfulFile(paths, s);
            });
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
                        bool success = _dbContext.SaveChanges() > 0 ? true : false;
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



            this.WhenActivated(async disposables =>
            {
                _service.WhenAnyValue(x => x.IsEditMode)
                    .BindTo(this, x => x.IsEditMode)
                    .DisposeWith(disposables);

                //Этап заполнения словаря с доп информацией
                if (File.Exists("SensorMoreData.json"))
                {
                    var list = _json.ReadFromJsonFile<List<AdditionalData>>("SensorMoreData.json");
                    _jsonMoreDataCache = list?.Where(x => x.Name != null).ToDictionary(x => x.Name, x => x) ?? new();
                }
                //Этап заполнения древовидной структуры названиями
                using (var _dbContext = _appDbContextFactory.CreateDbContext())
                {
                    var queryTypes = await _dbContext.SensorTypes
                            .AsNoTracking()
                            .Select(x => new SensorType()
                            {
                                Id = x.Id,
                                Name = x.Name,
                                Characteristics = x.Characteristics
                            })
                            .ToListAsync();

                    var querySensors = await _dbContext.Sensors
                        .AsNoTracking()
                        .Select(x => new Sensor()
                        {
                            Id = x.Id,
                            Name = x.Name,
                            SensorTypeID = x.SensorTypeID
                        })
                        .ToListAsync();

                    sensorTypes = new(queryTypes);
                    Sensors = new(querySensors);

                    Func<SensorType, Sensor, bool> filter = (type, sensor) => sensor.SensorTypeID == type.Id;
                    SensorsTree = new TreeViewCollection<SensorType, Sensor>("Name", new(sensorTypes), Sensors, filter);
                    
                }

                //Этап фильтрации механизмов
                this.WhenAnyValue(x => x.SelectedNode)
                    .Where(sensor => sensor != null)
                    .SelectMany(async sensor =>
                    {
                        // Создаем временный контекст ИМЕННО на время выполнения этого запроса
                        using var dbContext = appDbContextFactory.CreateDbContext();

                        // Все запросы делаем через локальный dbContext
                        return await dbContext.Mechanisms
                            .AsNoTracking()
                            .Where(mech => mech.MapObjects
                                .OfType<SensorAssignments>()
                                .Any(sa => sa.SensorId == sensor.Id))
                            .Select(x => new Mechanism
                            {
                                Id = x.Id,
                                Name = x.Name,
                                SectorID = x.SectorID,
                                MapObjects = new(x.MapObjects.Select(k => new SensorAssignments() { MechanismId = k.MechanismId }).ToList())
                            })
                            .ToListAsync(); // Асинхронное выполнение
                    })
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(filteredList =>
                    {
                        FilteredMechanisms = new(filteredList);
                    })
                    .DisposeWith(disposables);


                this.WhenAnyValue(x => x.SelectedNode)
                    .Where(sensor => sensor != null)
                    .SelectMany(async selectedSensor =>
                    {
                        // 1. Сначала подготавливаем AdditionalData (синхронно в фоновом таске)
                        if (selectedSensor.AdditionalData == null)
                        {
                            if (_jsonMoreDataCache.TryGetValue(selectedSensor.Name, out var cachedData) && cachedData.HasData())
                            {
                                selectedSensor.AdditionalData = cachedData;
                            }
                            else
                            {
                                var sType = sensorTypes.FirstOrDefault(t => t.Id == selectedSensor.SensorTypeID);
                                if (sType != null && sType.Characteristics?.Any() == true)
                                {
                                    selectedSensor.AdditionalData = AdditionalData.CreateRecord(selectedSensor.Name, sType.Characteristics);
                                    selectedSensor.AdditionalData.Data = new(sType.Characteristics.Select(c => new MoreData
                                    {
                                        Parameter = c.Title,
                                        Value = string.Empty
                                    }));
                                }
                            }
                        }

                        // 2. Проверяем, нужно ли лезть в БД за тяжелыми данными
                        if (selectedSensor.Image != null || selectedSensor.Files?.Any() == true)
                        {
                            return selectedSensor; // Данные уже есть, возвращаем датчик
                        }

                        // 3. Догружаем тяжелые данные из БД
                        using var dbContext = _appDbContextFactory.CreateDbContext();
                        var heavyData = await dbContext.Sensors
                            .AsNoTracking()
                            .Where(s => s.Id == selectedSensor.Id)
                            .Select(s => new { s.Image, s.Files, s.SensorType })
                            .FirstOrDefaultAsync();

                        if (heavyData != null)
                        {
                            selectedSensor.Image = heavyData.Image;
                            selectedSensor.Files = heavyData.Files;
                            selectedSensor.SensorType = heavyData.SensorType;
                        }
                        return selectedSensor;
                    })
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(sensor =>
                    {
                        this.RaisePropertyChanged(nameof(SelectedNode));
                    })
                    .DisposeWith(disposables);
                

                
                
                
                
            });
        }
        
        public ICommand SaveMoreData { get; }
        public ICommand AddFiles { get; }
        public ICommand DeletePathFiles { get; }
        public ICommand SaveFiles { get; }
        public ICommand ShowAllFiles { get; }
        public ICommand OpenFile { get; }
        public ICommand NavigateToMech {  get; }
        public ICommand OpenFullScreen { get; }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

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
