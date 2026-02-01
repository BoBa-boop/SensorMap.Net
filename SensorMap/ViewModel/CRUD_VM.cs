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
        private bool isEditMode;
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
        [Reactive] public ObservableCollection<Mechanism> Mechanisms { get; set; }

        public CRUD_VM(IDataBaseProvider provider,IDataService service,IAppDbContextFactory cxFactory,INavigation nav,ITempImage tempImage) 
        {
            Navigation = nav;
            _tempImage = tempImage;
            _provider = provider;
            _service = service;

            Sectors = _service.Sectors;
            Mechanisms = _service.Mechanisms;
            PLCs = _service.PLCs;
            //Manufacturers = _service.PLC_Manufacturers;
            Sensors = _service.Sensors;
            SensorTypes = _service.SensorTypes;
            
            ShowCommand =new RelayCommand<object>((Sensor)=> 
            {
                if(Sensor is Sensor) 
                    Navigation.NavigateTo<SensorVM>(Sensor); 
            });
            SaveCommand = new RelayCommand<object>((arg) =>
            {
                if (arg is null) return;
                var entityType = arg.GetType();

                PropertyInfo? idProperty = entityType.GetProperty("Id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (idProperty != null && Convert.ToInt32(idProperty.GetValue(arg)) == 0)
                {
                    MethodInfo createMethod = typeof(IDataBaseProvider).GetMethod(nameof(_provider.Create))!.MakeGenericMethod(entityType);
                    if (createMethod.Invoke(_provider, new object[] { arg }) != null)
                    {
                        entityType?.GetProperty("IsModified")?.SetValue(arg, false);
                    }
                }
                else
                {
                    MethodInfo updateMethod = typeof(IDataBaseProvider).GetMethod(nameof(_provider.Update))!.MakeGenericMethod(entityType);
                    if (updateMethod.Invoke(_provider, new object[] { arg }) != null)
                        entityType?.GetProperty("IsModified")?.SetValue(arg, false);
                }
            });
            DeleteCommand = new RelayCommand<object>((arg) => 
            {
                if (arg is null) return;
                var entityType = arg.GetType();
                MethodInfo deleteMethod = typeof(IDataBaseProvider).GetMethod(nameof(_provider.Delete))!.MakeGenericMethod(entityType);
                if (deleteMethod.Invoke(_provider, new object[] { arg }) != null)
                {
                    PropertyInfo? prop = typeof(CRUD_VM).GetProperty(entityType.Name + "s");
                    if(prop!=null)
                    {
                        var collection = prop.GetValue(this) as System.Collections.IList;
                        collection?.Remove(arg);
                    }
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
                    if (_service.SensorTypes.Contains(sensorType))
                        DeleteCommand.Execute(type); 
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
            //DataBaseEvents.EntityCreated.Subscribe(OnEntityCreated);
            //DataBaseEvents.EntityDeleted.Subscribe(OnEntityDeleted);
            //DataBaseEvents.EntityUpdated.Subscribe(OnEntityUpdated);
        }

        private void OnEntityUpdated(DataBaseEvents.TEntityEvent @event)
        {
            switch (@event.EntityType.Name)
            {
                case nameof(Sector):
                    var newSector = Sectors.FirstOrDefault(s => s.Id == @event.Id);
                    foreach (var mech in _service.Mechanisms.Where(meh => meh.Id == @event.Id).ToList())
                    {
                        mech.Sector = newSector;
                    }

                    break;
                case nameof(Mechanism):
                    var CurrentSector = Sectors.FirstOrDefault(s => s.Id == @event.Id);
                    if (CurrentSector != null) CurrentSector.Mechanisms = new(Mechanisms.Where(meh => meh.Id == @event.Id).ToList());
                    break;
                default:
                    break;
            }
        }

        private void OnEntityDeleted(DataBaseEvents.TEntityEvent @event)
        {
            switch (@event.EntityType.Name)
            {
                case nameof(Sector):
                    
                    foreach (var mech in _service.Mechanisms.Where(sect => sect.Id == @event.Id).ToList())
                    {
                        mech.Sector = null;
                    }

                    break;
                case nameof(Mechanism):
                    // Обновляем коллекцию механизмов
                    break;
                default:
                    break;
            }
        }

        private void OnEntityCreated(DataBaseEvents.TEntityEvent @event)
        {
            throw new NotImplementedException();
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
