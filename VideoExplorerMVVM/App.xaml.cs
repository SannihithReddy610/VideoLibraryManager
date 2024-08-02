#region Directives
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VideoLibraryManager.View;
using VideoLibraryManager.ViewModel;
#endregion

namespace VideoLibraryManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Constructor
        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
            _logger = ServiceProvider.GetRequiredService<ILogger<App>>();
        }
        #endregion

        #region Properties
        public IServiceProvider ServiceProvider { get; private set; }
        #endregion

        #region Private Methods
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddDebug());
            services.AddSingleton<VideoManagerViewModel>();
            services.AddSingleton<MainWindow>();
        }
        #endregion

        #region Protected Methods
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                _mutex = new Mutex(true, "$VideoLibraryManager$", out var isNewInstance);
                if (!isNewInstance)
                {
                    _logger.LogWarning("Video Library Manager is already running.");
                    MessageBox.Show("Video Library Manager is already running...");
                    Current.Shutdown();
                    return;
                }

                var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during application startup.");
                MessageBox.Show("An unexpected error occurred. The application will now close.");
                Current.Shutdown();
            }
        }
        #endregion

        #region Private Fields
        private Mutex _mutex;
        private readonly ILogger _logger;
        #endregion
    }

}
