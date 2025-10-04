using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReactiveUI;
using SensorMap.EF;
using SensorMap.Interfaces;
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
        private ServiceProvider? _serviceProvider;
        protected override void OnStartup(StartupEventArgs e)
        {
            IServiceCollection services = new ServiceCollection();
            ConfigurationServiceces(services);
            AnalyzeDataBase();
            _serviceProvider = services.BuildServiceProvider();
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
            var config = new ConfigurationBuilder()
                       .AddJsonFile("appsettings.json")
                       .SetBasePath(Directory.GetCurrentDirectory())
                       .Build();
            string? connection_string = config.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connection_string))
            {
                DbContextOptions options = new DbContextOptionsBuilder().UseSqlite().UseSqlite(connection_string).Options;
                using (AppDBContext dBContext = new AppDBContext(options))
                {
                    dBContext.Database.Migrate();
                }
            }
            else
            {
                MessageBox.Show("Отсутсвует путь к БД. Установите его в настройках","Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
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

            //Регистрация VM
            services.AddSingleton<MenuVM>();
            services.AddTransient<MenuButtonsVM>();
            services.AddTransient<MechanismVM>();
            services.AddTransient<SensorVM>();
            services.AddTransient<SectorsVM>();
            services.AddTransient<SettingsVM>();

            //Регистрация сервисы
            services.AddSingleton<IDataService, DataService>();
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
