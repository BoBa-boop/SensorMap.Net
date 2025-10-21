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
            //AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            //{
            //    MessageBox.Show($"Fatal error: {args.ExceptionObject}");
            //    Environment.Exit(1);
            //};

            //DispatcherUnhandledException += (s, args) =>
            //{
            //    MessageBox.Show($"UI error: {args.Exception.Message}");
            //    args.Handled = true;
            //};
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
                dBContext.Database.MigrateAsync();
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
                MessageBox.Show("Отсутствует путь к БД. Установите его в настройках", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
        
    }

}
