using System.Windows;
namespace VideoLibraryManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        Mutex _mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            bool aIsNewInstance;
            _mutex = new Mutex(true, "$VideoLibraryManager$", out aIsNewInstance);
            if (aIsNewInstance) return;
            MessageBox.Show("Video Library Manager is running...");
            App.Current.Shutdown();
        }
    }

}
