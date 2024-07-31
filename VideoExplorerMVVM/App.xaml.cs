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
            // Configure services
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
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
            base.OnStartup(e);
            _mutex = new Mutex(true, "$VideoLibraryManager$", out var aIsNewInstance);
            if (!aIsNewInstance)
            {
                MessageBox.Show("Video Library Manager is already running...");
                Current.Shutdown();
            }

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

        }

        private Mutex _mutex;

        #endregion
    }

}
