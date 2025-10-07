using DynamicData.Kernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReactiveUI;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using SensorMap.View;
using SensorMap.ViewModel;
using System.IO;
using System.Reflection;
using System.Windows;

namespace SensorMap
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected Mutex? Mutex;
        private ServiceProvider _serviceProvider = null!;
        protected override void OnStartup(StartupEventArgs e)
        {
            IServiceCollection services = new ServiceCollection();
            ConfigurationServiceces(services);
            _serviceProvider = services.BuildServiceProvider();

            AnalyzeDataBase();
            if (IsStartOneApp(e))
            {
                var menuApp = _serviceProvider.GetRequiredService<MenuView>();
                var navigation = _serviceProvider.GetRequiredService<INavigation>();
                navigation.SetMainWindow(menuApp);
                menuApp.Show();
            }
        }

        private void AnalyzeDataBase()
        {
            IAppDbContextFactory dbContextFactory = _serviceProvider.GetRequiredService<IAppDbContextFactory>();
            using (AppDBContext dBContext = dbContextFactory.CreateDbContext())
            {
                dBContext.Database.EnsureCreated();
                dBContext.Database.MigrateAsync();
                //InitializeTestData(dBContext);
            }


        }

        private static void ConfigurationServiceces(IServiceCollection services)
        {
            services.AddSingleton<MenuView>(provider =>
            {
                var window = new MenuView();
                window.DataContext = provider.GetRequiredService<MenuVM>();
                return window;
            });
            //Регистрация окон
            services.AddTransient<SensorView>();
            services.AddTransient<MechanismView>();
            services.AddTransient<CRUD_View>();

            //Регистрация VM
            services.AddSingleton<MenuVM>();
            services.AddTransient<MenuButtonsVM>();
            services.AddTransient<MechanismVM>();
            services.AddTransient<SensorVM>();
            services.AddTransient<SectorsVM>();
            services.AddTransient<CRUD_VM>();
            services.AddTransient<SettingsVM>();

            //Регистрация сервисы
            ConfigurationDataBase(services);
            services.AddSingleton<IDataService, DataService>();
            services.AddSingleton<IDataBaseProvider, DataBaseProvider>();
            services.AddSingleton<INavigation, NavigationService>();



            // Регистрация для создания VM без параметров
            services.AddSingleton<Func<Type, ReactiveObject>>(serviceProvider => viewModelType =>
            (ReactiveObject)serviceProvider.GetRequiredService(viewModelType));

            // Регистрация для создания VM с параметрами
            services.AddSingleton<Func<Type, object, ReactiveObject>>(provider => (type, parameter) =>
            {
                var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

                foreach (var constructor in constructors)
                {
                    var parameters = constructor.GetParameters();

                    // Создаем массив параметров для Invoke
                    var resolvedArguments = new List<object>();

                    foreach (var paramInfo in parameters)
                    {
                        if (paramInfo.ParameterType.IsAssignableFrom(parameter.GetType()))
                            resolvedArguments.Add(parameter); // Добавляем внешний параметр
                        else
                            resolvedArguments.Add(provider.GetRequiredService(paramInfo.ParameterType)); // Разрешаем интерфейс или другой тип через DI
                    }

                    return (ReactiveObject)constructor.Invoke(resolvedArguments.ToArray());
                }
                throw new InvalidOperationException("Конструктор не найден");
            });

        }

        private static void ConfigurationDataBase(IServiceCollection services)
        {
            var config = new ConfigurationBuilder()
                                   .AddJsonFile("appsettings.json")
                                   .SetBasePath(Directory.GetCurrentDirectory())
                                   .Build();
            string? connection_string = config.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connection_string))
                services.AddSingleton<IAppDbContextFactory>(new DBContextFactory(connection_string));
            else
            {
                MessageBox.Show("Отсутсвует путь к БД. Установите его в настройках", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsStartOneApp(StartupEventArgs e)
        {
            Mutex = new Mutex(true, ResourceAssembly.GetName().Name);
            if (!Mutex.WaitOne())
            {
                Current.Shutdown();
                MessageBox.Show("Приложение уже запущено!", "Ошибка запуска", MessageBoxButton.OK);
                return false;
            }
            else
            {
                return true;
            }
        }
        public void InitializeTestData(AppDBContext dBContext)
        {
            try
            {
                // 1. Создаем участки
                var sectors = new[]
                {
                    new Sector { Name = "2 нарезная линия"},
                    new Sector { Name = "3 нарезная линия"}
                };
                dBContext.Sectors.AddRange(sectors);
                dBContext.SaveChanges();

                // 3. Создаем механизмы
                var mechanisms = new[]
                {
                    new Mechanism { Name = "Гидравлический пресс MDM", Path = "press_mechanism", Sector = sectors[0], Image = "/Resources/Схема МДМ.jpg" },
                    new Mechanism { Name = "Шаблон", Path = "conveyor", Sector = sectors[0], Image = "/Resources/Шаблон.jpg"},
                    new Mechanism { Name = "Механизация станков", Path = "robot", Sector = sectors[0], Image = "/Resources/2 нар линия механизация.jpg"}
                };
                dBContext.Mechanisms.AddRange(mechanisms);
                dBContext.SaveChanges();

                // 2. Создаем PLC
                var plcs = new[]
                {
                    new PLC { TypePLC = "Siemens S7-1200", IP = "192.168.1.10", Image = "plc_siemens.png",Mechanism=mechanisms[0]  },
                    new PLC { TypePLC = "Allen-Bradley", IP = "192.168.1.11", Image = "plc_ab.png" ,Mechanism=mechanisms[1]},
                    new PLC { TypePLC = "Schneider Electric", IP = "192.168.1.12", Image = "plc_schneider.png",Mechanism=mechanisms[2] }
                };
                dBContext.PLCs.AddRange(plcs);
                dBContext.SaveChanges();



                // 4. Создаем датчики (каталог)
                var sensors = new[]
                {
                    new Sensor { Name = "Датчик давления ПД-100", Type = Sensor.SensorType.Давления,Image = "pressure_sensor.png" },
                    new Sensor { Name = "Индуктивный датчик ИД-5", Type = Sensor.SensorType.Индуктивный, Image = "inductive_sensor.png" },
                    new Sensor { Name = "Оптический барьер ОБ-2", Type = Sensor.SensorType.Оптический, Image = "optical_sensor.png" },
                    new Sensor { Name = "Концевой выключатель КВ-1", Type = Sensor.SensorType.Концевик, Image = "limit_switch.png" },
                    new Sensor { Name = "Энкодер ЭН-500", Type = Sensor.SensorType.Энкодер, Image = "encoder.png" },
                    new Sensor { Name = "Лазерный дальномер ЛД-10", Type = Sensor.SensorType.Лазерный, Image = "laser_sensor.png" },
                    new Sensor { Name = "Линейный датчик ЛП-25", Type = Sensor.SensorType.Линейка, Image = "linear_sensor.png" },
                    new Sensor { Name = "Герконовый датчик ГД-3", Type = Sensor.SensorType.Геркон, Image = "reed_switch.png" }
                };
                dBContext.Sensors.AddRange(sensors);
                dBContext.SaveChanges();

                // 5. Создаем входы PLC
                var plcInputs = new List<PLCInputs>();

                // Для каждого PLC создаем несколько входов
                foreach (var plc in plcs)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        plcInputs.Add(new PLCInputs
                        {
                            Address = $"I{plc.Id}.{i}",
                            Location = $"находиться там {i}",
                            SensorId = 2,
                            PLCId =plc.Id
                        });
                    }

                }
                dBContext.SensorAssignments.AddRange(plcInputs);
                dBContext.SaveChanges();

            }
            catch
            {
                throw;
            }
        }
    }

}
