using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Behaviors;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SensorMap.ViewModel
{
    public class CRUD_VM:ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        private readonly IDataService _service; 
        private readonly ITempImage _tempImage;
        private IAppDbContextFactory _appDbContextFactory;
        private AppDBContext dBContext;
        private bool isEditMode;
        private ObservableCollection<Mechanism> _mech;
        public readonly UndoRedoStack _undoRedoManager = new UndoRedoStack();
        [Reactive] public bool IsEditMode { get => isEditMode; set { this.RaiseAndSetIfChanged(ref isEditMode, value); } }
        [Reactive] public bool CanUndo => _undoRedoManager.CanUndo;
        [Reactive] public bool CanRedo=>_undoRedoManager.CanRedo;
        [Reactive] public INavigation Navigation { get; set; }
        [Reactive] public ObservableCollection<Sector> Sectors { get; set; }
        [Reactive] public ObservableCollection<Sensor> Sensors { get; set; }
        [Reactive] public ObservableCollection<PLC> PLCs { get; set; }
        [Reactive] public ObservableCollection<SensorType> SensorTypes { get; set; }
        [Reactive] public ObservableCollection<string> Manufacturers { get; set; }
        [Reactive] public ObservableCollection<Mechanism> Mechanisms
        {
            get => _mech;
            set
            {
                this.RaiseAndSetIfChanged(ref _mech, value);
            }
        }

        public CRUD_VM(IDataBaseProvider provider,IDataService service,IAppDbContextFactory cxFactory,INavigation nav,ITempImage tempImage) 
        {
            Navigation = nav;
            _tempImage = tempImage;
            _provider = provider;
            _appDbContextFactory = cxFactory;
            dBContext = _appDbContextFactory.CreateDbContext();
            _service = service;
            Sectors = new (dBContext.Sectors.ToList());
            Mechanisms = new (dBContext.Mechanisms.ToList());
            PLCs = new(dBContext.PLCs.ToList());
            Manufacturers = new(PLCs.Select(plc => plc.Manufacturer).Distinct().ToList());
            Sensors = new(dBContext.Sensors.ToList());
            SensorTypes = new(dBContext.SensorTypes.ToList());
            
            ShowCommand =new RelayCommand<object>((Sensor)=> 
            {
                if(Sensor is Sensor) 
                    Navigation.NavigateTo<SensorVM>(Sensor); 
            });
            SaveCommand = new RelayCommand<object>((arg) =>
            {
                if (arg is null) return;
                
                dBContext.Update(arg);
                arg.GetType()?.GetProperty("IsModified")?.SetValue(arg, false);
                dBContext.SaveChanges();
                
            });
            DeleteCommand = new RelayCommand<object>((arg) => 
            {
                if (arg is null) return;
                var entityType = arg.GetType();
                
                PropertyInfo? prop = typeof(CRUD_VM).GetProperty(entityType.Name + "s");
                if (prop != null)
                {
                    var collection = prop.GetValue(this) as System.Collections.IList;
                    collection?.Remove(arg);
                }
                if(dBContext.Entry(arg).State!=EntityState.Detached)
                {
                    dBContext.Remove(arg);
                    dBContext.SaveChanges();
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
                if(image is byte[] img && img!=null)
                {
                    var browser = new CustomImageBrowser(_tempImage.CreateImageFromBytes(img)) {Title="Просмотр схемы" };
                    browser.ShowDialog();
                }
            });
            AddSensorType = new RelayCommand<object>((param) => 
            {
                if(param is null) return;

                var values = (object[])param;
                var name = (string)values[0];
                var image = (Uri)values[1];
                byte[] photoBytes;
                SensorType sType = new SensorType();
                sType.Name = name;

                if (image!= null)
                {
                    using (FileStream fs = new FileStream(image.LocalPath, FileMode.Open, FileAccess.Read))
                    {
                        photoBytes = new byte[fs.Length];
                        fs.ReadExactly(photoBytes);
                    }
                    sType.Image = photoBytes;
                }
                if(!SensorTypes.Where(x=>x.Name==sType.Name).Any())
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
            DeleteNodeTitleType = new RelayCommand<object>((type) => 
            {
                if(type is SensorType sensorType&& sensorType!=null)
                {
                    SensorTypes.Remove(sensorType);
                    if (dBContext.SensorTypes.Contains(sensorType))
                    {
                        dBContext.SensorTypes.Remove(sensorType);
                        dBContext.SaveChanges();
                    }
                }
            }, (type) => { return type != null; });

            UndoCommand = new RelayCommand(_undoRedoManager!.Undo);
            RedoCommand = new RelayCommand(_undoRedoManager.Redo);

            _service.WhenAnyValue(x => x.IsEditMode)
                .BindTo(this, x => x.IsEditMode);

            _undoRedoManager.WhenAnyValue(x => x.CanUndo)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(CanUndo)));

            _undoRedoManager.WhenAnyValue(x => x.CanRedo)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(CanRedo)));
        }

        

        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand ShowCommand { get; set; }
        public ICommand AddImage {  get; set; }
        public ICommand ShowPreviewImage { get; set; }
        public ICommand AddSensorType { get; }
        public ICommand DeleteNodeTitleType { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public void RecordEdit<T>(T _inputObject, string propertyName, object oldValue, object newValue)
        {
            var command = new Commands.DataGridCommands.EditCell<T>(_inputObject, propertyName, oldValue, newValue);
            _undoRedoManager.Do(command);
        }
        
    }
}
