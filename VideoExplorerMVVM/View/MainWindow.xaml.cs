#region Directives
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VideoLibraryManager.Model;
using VideoLibraryManager.ViewModel;

#endregion

namespace VideoLibraryManager.View
{
    public partial class MainWindow : Window
    {

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// Sets up the DataContext, initializes the Timer, and registers event handlers.
        /// </summary>
        public MainWindow(VideoManagerViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            SetDataContextAndMediaElement();
            InitializeTimer();
            RegisterEventHandlers();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Sets DataContext for the MainWindow to ViewModel.
        /// </summary>
        private void SetDataContextAndMediaElement()
        {
            DataContext = _viewModel;
            _viewModel.MediaElement = mediaElement;
        }

        /// <summary>
        /// Initializes the DispatcherTimer for updating UI elements based on media playback.
        /// </summary>
        private void InitializeTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Registers DataContextChanged event to handle changes to the DataContext of the MainWindow
        /// </summary>
        private void RegisterEventHandlers()
        {
            DataContextChanged += MainWindow_DataContextChanged;
            Loaded += MainWindow_Loaded;
            mediaElement.MediaOpened += MediaElement_MediaOpened;
            mediaElement.MediaEnded += MediaElement_MediaEnded;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        }

        /// <summary>
        /// Handles the events when the main window is loaded.
        /// </summary>
        /// <param name="sender">The source of the event, typically the main window.</param>
        /// <param name="e">The event data associated with the loaded event.</param>
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is VideoManagerViewModel viewModel)
            {
                await viewModel.LoadVideosCommand.ExecuteAsync(null);
                await viewModel.LoadCloudVideosCommand.ExecuteAsync(null);
            }
        }

        /// <summary>
        /// Handles changes to the DataContext of the MainWindow.
        /// Updates the ViewModel and subscribes/unsubscribes to its PropertyChanged event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments containing old and new values of DataContext.</param>
        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is VideoManagerViewModel oldViewModel)
            {
                oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            if (e.NewValue is VideoManagerViewModel newViewModel)
            {
                newViewModel.PropertyChanged += ViewModel_PropertyChanged;
                newViewModel.MediaElement = mediaElement;
            }
        }

        /// <summary>
        /// Handles the SelectedItemChanged event of the TreeView.
        /// Updates the SelectedVideo property of the ViewModel based on the selected item.
        /// </summary>
        /// <param name="sender">The sender of the event (TreeView).</param>
        /// <param name="e">The event arguments containing the old and new selected items.</param>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = e.NewValue;

            if (selectedItem is VideoFile selectedVideoFile)
            {
                _viewModel.SelectedVideo = selectedVideoFile;
            }
            else if (selectedItem is CloudVideoFile selectedCloudVideoFile)
            {
                _viewModel.CloudSelectedVideo = selectedCloudVideoFile;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event for the ViewModel.
        /// Updates the video playback when the SelectedVideo property changes.
        /// </summary>
        /// <param name="sender">The sender of the event (ViewModel).</param>
        /// <param name="e">The event arguments containing the name of the changed property.</param>
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VideoManagerViewModel.IsFullScreen))
            {
                if ((sender as VideoManagerViewModel).IsFullScreen)
                {
                    EnterFullScreen();
                }
                else
                {
                    ExitFullScreen();
                }
            }
        }

        /// <summary>
        /// Handles the MediaOpened event of the MediaElement.
        /// Initializes the seek bar's maximum value based on the media duration.
        /// </summary>
        /// <param name="sender">The sender of the event (MediaElement).</param>
        /// <param name="e">The event arguments.</param>
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                seekBar.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                _timer.Start();
            }
        }

        /// <summary>
        /// Handles the MediaEnded event of the MediaElement.
        /// Stops media playback and stops the timer when the media ends.
        /// </summary>
        /// <param name="sender">The sender of the event (MediaElement).</param>
        /// <param name="e">The event arguments.</param>
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            _viewModel.StopCommand.Execute(null);
            _timer.Stop();
        }

        /// <summary>
        /// Handles the ValueChanged event of the seek bar.
        /// Updates the media element's position based on the seek bar's value.
        /// </summary>
        /// <param name="sender">The sender of the event (SeekBar).</param>
        /// <param name="e">The event arguments containing the new value of the seek bar.</param>
        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaElement != null && Math.Abs(mediaElement.Position.TotalSeconds - seekBar.Value) > 1)
            {
                mediaElement.Position = TimeSpan.FromSeconds(seekBar.Value);
            }
        }

        /// <summary>
        /// Handles the ValueChanged event of the volume slider.
        /// Updates the media element's volume based on the slider's value.
        /// </summary>
        /// <param name="sender">The sender of the event (VolumeSlider).</param>
        /// <param name="e">The event arguments containing the new value of the volume slider.</param>
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaElement != null)
            {
                mediaElement.Volume = VolumeSlider.Value;
            }
        }

        /// <summary>
        /// Handles the Tick event of the timer.
        /// Updates the seek bar and video duration display based on the current media playback position.
        /// </summary>
        /// <param name="sender">The sender of the event (Timer).</param>
        /// <param name="e">The event arguments.</param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            var viewModel = DataContext as VideoManagerViewModel;
            if (viewModel != null && mediaElement.Source != null && mediaElement.NaturalDuration.HasTimeSpan)
            {
                viewModel.SeekBarValue = mediaElement.Position.TotalSeconds;
                viewModel.VideoDuration = mediaElement.Position.ToString(@"hh\:mm\:ss") + " / " + mediaElement.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss");
            }
        }

        /// <summary>
        /// Enters full-screen mode, storing the current window state, style, and resize mode.
        /// </summary>
        private void EnterFullScreen()
        {
            _previousWindowState = this.WindowState;
            _previousWindowStyle = this.WindowStyle;
            _previousResizeMode = this.ResizeMode;

            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;

            mediaElement.Width = SystemParameters.PrimaryScreenWidth;
            mediaElement.Height = SystemParameters.PrimaryScreenHeight;
            _isFullScreen = true;

            this.KeyDown += FullScreen_KeyDown;
        }

        /// <summary>
        /// Exits full-screen mode, restoring the previous window state, style, and resize mode.
        /// </summary>
        private void ExitFullScreen()
        {
            this.WindowState = _previousWindowState;
            this.WindowStyle = _previousWindowStyle;
            this.ResizeMode = _previousResizeMode;

            mediaElement.Width = double.NaN; 
            mediaElement.Height = double.NaN; 
            _isFullScreen = false;

            this.KeyDown -= FullScreen_KeyDown;
        }

        /// <summary>
        /// Handles the keydown event to exit full-screen mode when the Escape key is pressed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The key event arguments.</param>
        private void FullScreen_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _isFullScreen)
            {
                var viewModel = DataContext as VideoManagerViewModel;
                viewModel.IsFullScreen = false;
            }
        }

        /// <summary>
        /// Handle current domain unhandled Exceptions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            
        }
        #endregion

        #region Private Fields
        private DispatcherTimer _timer;
        private readonly VideoManagerViewModel _viewModel;
        private bool _isFullScreen = false;
        private WindowState _previousWindowState;
        private WindowStyle _previousWindowStyle;
        private ResizeMode _previousResizeMode;
        #endregion
        
    }
}
