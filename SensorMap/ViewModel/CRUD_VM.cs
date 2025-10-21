using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.View;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows.Input;

namespace SensorMap.ViewModel
{
    public class CRUD_VM:ReactiveObject
    {
        private ObservableCollection<Sector> _sectors = new();
        public ObservableCollection<Sensor> _sensors = new();
        public ObservableCollection<Mechanism> _mechanisms = new();

        private readonly IDataBaseProvider _provider;
        private readonly IDataService _service;
        [Reactive] public INavigation Navigation { get; set; }
        [Reactive] public ObservableCollection<Sector> Sectors 
        {
            get => _sectors;
            set => this.RaiseAndSetIfChanged(ref _sectors, value);
        }
        [Reactive] public ObservableCollection<Sensor> Sensors
        {
            get => _sensors;
            set => this.RaiseAndSetIfChanged(ref _sensors, value);
        }
        [Reactive] public ObservableCollection<Mechanism> Mechanisms
        {
            get => _mechanisms;
            set => this.RaiseAndSetIfChanged(ref _mechanisms, value);
        }
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
        }

        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand ShowCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand AddImage {  get; set; }

    }
}
