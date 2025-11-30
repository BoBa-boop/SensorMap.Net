using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Behaviors;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using SensorMap.View;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reactive.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Windows.Input;
using System.Windows.Media;

namespace SensorMap.ViewModel
{
    public class CRUD_VM:ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        private readonly IDataService _service;
        private readonly ITempImage _tempImage;

        [Reactive] public bool IsEditMode { get; set; }
        [Reactive] public INavigation Navigation { get; set; }
        [Reactive] public ObservableCollection<Sector> Sectors { get; set; }
        [Reactive] public ObservableCollection<Sensor> Sensors { get; set; }
        [Reactive] public ObservableCollection<PLC> PLCs { get; set; }
        [Reactive] public ObservableCollection<SensorType> SensorTypes { get; set; }
        [Reactive] 
        public ObservableCollection<SensorType> TempSensorTypes { get; set; }
        [Reactive] public ObservableCollection<Mechanism> Mechanisms { get; set; }

        public CRUD_VM(IDataBaseProvider provider,IDataService service,INavigation nav,ITempImage tempImage,bool _IsEditMode=false) 
        {
            IsEditMode = _IsEditMode;
            Navigation = nav;
            _tempImage = tempImage;
            _provider = provider;
            _service = service;
            Sectors = _service.Sectors;
            PLCs = _service.PLCs;
            Sensors = _service.Sensors;
            SensorTypes = _service.SensorTypes;
            TempSensorTypes = new(SensorTypes);
            Mechanisms = _service.Mechanisms;
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
                    var collection = prop.GetValue(this) as System.Collections.IList;
                    collection?.Remove(arg);
                }
            });
            CancelCommand = new RelayCommand<object>((arg)=>
            {
                if (arg is null) return;
                var entityType = arg.GetType();
                entityType.GetMethod("CancelEdit")!.Invoke(arg, null);
                entityType?.GetProperty("IsModified")?.SetValue(arg, false);


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
                        fs.Read(photoBytes, 0, photoBytes.Length);
                        entityType.GetProperty("Image")?.SetValue(arg, photoBytes);
                    }
                }
            });
            ShowPreviewImage = new RelayCommand<object>((image) => 
            {
                if(image as byte[] == null) return;
                var browser = new CustomImageBrowser(_tempImage.CreateImageFromBytes(image as byte[])) {Title="Просмотр схемы" };
                browser.ShowDialog();
            });
            AddNodeTitleType = new RelayCommand<object>((param) => 
            {
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
                        fs.Read(photoBytes, 0, photoBytes.Length);
                    }
                    sType.Image = photoBytes;
                }
                if(!TempSensorTypes.Where(x=>x.Name==sType.Name).Any())
                {
                    sType.IsNew = true;
                    TempSensorTypes.Add(sType);
                }
            }, (param) => { var values = (object[]?)param; return !string.IsNullOrWhiteSpace((string)values[0]); });
            DeleteNodeTitleType = new RelayCommand<object>((type) => { TempSensorTypes.Remove(type as SensorType); }, (type) => { return type != null; });
            
        }

        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand ShowCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand AddImage {  get; set; }
        public ICommand ShowPreviewImage { get; set; }
        public ICommand AddNodeTitleType { get; }
        public ICommand SaveNodeTitleType { get; }
        public ICommand DeleteNodeTitleType { get; }
        
    }
}
