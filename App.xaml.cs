using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Global_Insights_Dashboard.ViewModels;
using Global_Insights_Dashboard.Services.Interfaces;
using Global_Insights_Dashboard.Services;
using Global_Insights_Dashboard.Models.Configuration;
using MaterialDesignThemes.Wpf;

namespace Global_Insights_Dashboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;
        public IConfiguration? Configuration { get; private set; }

        /// <summary>
        /// Public access to the service provider for dependency injection
        /// </summary>
        public static ServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure services
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            ServiceProvider = _serviceProvider;

            // Set up global exception handling
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // Initialize and show main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
            services.AddSingleton(Configuration);

            // Configure strongly-typed settings
            services.Configure<ApiConfiguration>(Configuration.GetSection("ApiConfiguration"));
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            // HTTP Clients with timeout and retry policies
            services.AddHttpClient<IWeatherService, WeatherService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.Add("User-Agent", "Global-Insights-Dashboard/1.0");
            });

            services.AddHttpClient<INewsService, NewsService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.Add("User-Agent", "Global-Insights-Dashboard/1.0");
            });

            services.AddHttpClient<IFinanceService, FinanceService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.Add("User-Agent", "Global-Insights-Dashboard/1.0");
            });

            services.AddHttpClient<ITriviaService, TriviaService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.Add("User-Agent", "Global-Insights-Dashboard/1.0");
            });

            // Application Services
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IThemeService, ThemeService>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<WeatherViewModel>();
            services.AddTransient<NewsViewModel>();
            services.AddTransient<FinanceViewModel>();
            services.AddTransient<TriviaViewModel>();
            services.AddTransient<SettingsViewModel>();

            // Windows
            services.AddTransient<MainWindow>();
        }

        private static string GetConfigurationDirectory()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configDir = Path.Combine(appDataPath, "GlobalInsightsDashboard");
            
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);
                
            return configDir;
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Log the exception and show user-friendly message
            MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}", 
                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            e.Handled = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Log the exception
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"A critical error occurred: {ex.Message}", 
                              "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }

        public static T? GetService<T>() where T : class
        {
            return ((App)Current)?._serviceProvider?.GetService<T>();
        }
    }
}
