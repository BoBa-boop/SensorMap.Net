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
using System.Windows.Input;
using System.Windows.Media;

namespace SensorMap.ViewModel
{
    public class CRUD_VM:ReactiveObject
    {
        private readonly IDataBaseProvider _provider;
        private readonly IDataService _service;
        private readonly ITempImage _tempImage;

        [Reactive] public INavigation Navigation { get; set; }
        [Reactive] public ObservableCollection<Sector> Sectors { get; set; }
        [Reactive] public ObservableCollection<Sensor> Sensors { get; set; }
        [Reactive] public ObservableCollection<PLC> PLCs { get; set; }
        [Reactive] public ObservableCollection<SensorType> SensorTypes { get; set; }
        [Reactive] public ObservableCollection<Mechanism> Mechanisms { get; set; }
        [Reactive] public object SelectedTab { get; set; }
        
        public CRUD_VM(IDataBaseProvider provider,IDataService service,INavigation nav,ITempImage tempImage) 
        {
            Navigation = nav;
            _tempImage = tempImage;
            _provider = provider;
            _service = service;
            Sectors = _service.Sectors;
            PLCs = _service.PLCs;
            Sensors = _service.Sensors;
            SensorTypes = _service.SensorTypes;
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
                    if(createMethod.Invoke(_provider, new object[] { arg })!=null)
                    {
                        entityType?.GetProperty("IsModified")?.SetValue(arg, false);
                    }
                }
                else
                {
                    MethodInfo updateMethod = typeof(IDataBaseProvider).GetMethod(nameof(_provider.Update))!.MakeGenericMethod(entityType);
                    if(updateMethod.Invoke(_provider, new object[] { arg })!=null)
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
            AddNodeTitleType = new RelayCommand<string>((name) => { SensorTypes.Add(new SensorType() { Name = name }); }, (name) => { return !string.IsNullOrWhiteSpace(name); });
            DeleteNodeTitleType = new RelayCommand<object>((type) => { SensorTypes.Remove(type as SensorType); }, (type) => { return type != null; });
            this.WhenAnyValue(x => x.SelectedTab).ObserveOn(RxApp.MainThreadScheduler).Subscribe((s) =>
            {
                var wqe = s as TabControl;
                MessageBox.Show(wqe.Tag);
            });
        }

        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand ShowCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand AddImage {  get; set; }
        public ICommand ShowPreviewImage { get; set; }
        public ICommand AddNodeTitleType { get; }
        public ICommand DeleteNodeTitleType { get; }
    }
}
