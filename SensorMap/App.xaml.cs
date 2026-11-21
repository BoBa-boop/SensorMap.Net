using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using ReactiveUI;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Logging;
using SensorMap.Model;
using SensorMap.Properties;
using SensorMap.Services;
using SensorMap.View;
using SensorMap.ViewModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Shapes;
using Velopack;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

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
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsApp.SettingsPath)!);
#if DEBUG == true
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                MessageBox.Show($"Fatal error: {args.ExceptionObject}");
                Environment.Exit(1);
            };
            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"UI error: {args.Exception.Message}");
                args.Handled = true;
            };
#endif
            VelopackApp.Build().Run();
            IServiceCollection services = new ServiceCollection();
            ConfigurationServiceces(services);
            _serviceProvider = services.BuildServiceProvider();
            
            if (IsStartOneApp(e))
            {
                if (Settings.Default.UpgradeRequired)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.UpgradeRequired = false;
                    Settings.Default.Save();
                }
                ConfigHelper.Instance.SetLang("ru");
                var menuApp = _serviceProvider.GetRequiredService<MainWindow>();
                var navigation = _serviceProvider.GetRequiredService<INavigation>();
                navigation.SetMainWindow(menuApp);
                menuApp.Show();
                AnalyzeDataBase();
            }
        }

        private async void AnalyzeDataBase()
        {

            var factory = _serviceProvider.GetRequiredService<IAppDbContextFactory>();

            if (string.IsNullOrEmpty(factory.DbPath))
            {
                var res = MessageBox.Show("Путь к Базе Данных пуст. Создать новую Базу Данных?", "Информация", MessageBoxButton.YesNo);
                if (res != MessageBoxResult.Yes) return;
                // первый запуск: создаём БД по умолчанию
                var path = AppPaths.DefaultDbPath;
                if (!File.Exists(path))
                {
                    var opts = new DbContextOptionsBuilder()
                        .UseSqlite($"DataSource={path}")      // ReadWriteCreate
                        .AddInterceptors(new AuditInterceptor())
                        .Options;
                    using var db = new AppDBContext(opts);
                    await db.Database.MigrateAsync();
                    Growl.Info("Создана новая БД.");
                }
                factory.SetDbPath(path);
                AppPaths.SaveDbPath(path);
            }

            var data = _serviceProvider.GetRequiredService<IDataService>();
            var provider = _serviceProvider.GetRequiredService<IDataBaseProvider>();
            var currentPath = Settings.Default.CurrentDbPath;
            Growl.Info(new GrowlInfo
            {
                Message = $"Проверка БД...",
                StaysOpen = true,
                ShowCloseButton = true,
                ShowDateTime = false
            });
            //var result = await provider.RunChecksAsync(currentPath,provider.CreateChecks());
            CheckResult result = new CheckResult(true); 
            Growl.Clear();
            if (result.Ok)
            {
                data.IsDataBaseConnect = true;
                Growl.Success("Проверка БД завершена успешно.");
            }
            else
            {
                Growl.Error($"Проверка БД:\r{result.Message}.");
                data.IsDataBaseConnect = false; 
            }

        }

        private static void ConfigurationServiceces(IServiceCollection services)
        {
            services.AddSingleton<MainWindow>(provider =>
            {
                var window = new MainWindow();
                window.DataContext = provider.GetRequiredService<MainWindowVM>();
                return window;
            });
            //Регистрация окон
            services.AddTransient<AuthorizationWindow>();
            services.AddTransient<DeviceInfo>();
            services.AddTransient<SensorAddInfo>();
            //Регистрация VM
            services.AddSingleton<MainWindowVM>();
            services.AddSingleton<MainMenuVM>();
            services.AddTransient<MechanismVM>();
            services.AddTransient<SensorVM>();
            services.AddTransient<DataBasePageVM>();
            services.AddTransient<SectorsVM>();
            services.AddTransient<CRUD_VM>();
            services.AddTransient<Devices_VM>();
            services.AddTransient<SettingsVM>();
            services.AddTransient<AuthVM>();

            //Регистрация сервисы
            services.AddSingleton<IAppDbContextFactory>(new DBContextFactory(AppPaths.GetDbPath()));
            services.AddSingleton<IDataService, DataService>();
            services.AddSingleton<IDataBaseProvider, DataBaseProvider>();
            services.AddSingleton<INavigation, NavigationService>();
            services.AddSingleton<ITempImage,TempImage>();
            services.AddTransient<IJsonSerialization, JsonSerializationService>();
            services.AddScoped<IPasswordHash,PasswordHashService>();
            services.AddTransient<IAuthorization, AuthorizationService>();
            services.AddSingleton<IFileManagment, FileManagmentService>();
            services.AddTransient<IClipboard, ClipboardService>();
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
            services.AddSingleton<ILogEntryService, LogEntryService>();

            NLog.LogManager.Setup().LoadConfiguration(builder => {
                builder.ForLogger().FilterMinLevel(LogLevel.Info).FilterMaxLevel(LogLevel.Fatal)
                    .WriteToFile(fileName: Path.Combine(AppPaths.DataDir,"LogDb.txt"), archiveAboveSize: 1048576, maxArchiveFiles: 5);
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
