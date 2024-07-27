#region Directives
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VideoExplorerMVVM.Model;
using VideoExplorerMVVM.ViewModel;
#endregion

namespace VideoExplorerMVVM
{
    public partial class MainWindow : Window
    {

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// Sets up the DataContext, initializes the Timer, and registers event handlers.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new VideoExplorerViewModel();
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
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is VideoExplorerViewModel viewModel)
            {
                await viewModel.LoadVideosCommand.ExecuteAsync(null);
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
            if (e.OldValue is VideoExplorerViewModel oldViewModel)
            {
                oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            if (e.NewValue is VideoExplorerViewModel newViewModel)
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
            var selectedVideo = e.NewValue as VideoFile;
            if (selectedVideo != null)
            {
                _viewModel.SelectedVideo = selectedVideo;
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
            if (e.PropertyName == nameof(VideoExplorerViewModel.SelectedVideo))
            {
                PlayVideo(_viewModel.SelectedVideo);
            }
        }

        /// <summary>
        /// Starts playback of the specified video and updates the media element's source.
        /// </summary>
        /// <param name="video">The video file to play.</param>
        private void PlayVideo(VideoFile video)
        {
            if (video != null)
            {
                mediaElement.Source = new Uri(video.FilePath);
                mediaElement.Play();
                _timer.Start();
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
                mediaElement.Volume = volumeSlider.Value;
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
            var viewModel = DataContext as VideoExplorerViewModel;
            if (viewModel != null && mediaElement.Source != null && mediaElement.NaturalDuration.HasTimeSpan)
            {
                viewModel.SeekBarValue = mediaElement.Position.TotalSeconds;
                viewModel.VideoDuration = mediaElement.Position.ToString(@"hh\:mm\:ss") + " / " + mediaElement.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss");
            }
        }

        /// <summary>
        /// Handles the Click event of the Play MenuItem.
        /// Executes the PlayCommand for the currently selected video file.
        /// </summary>
        /// <param name="sender">The sender of the event (MenuItem).</param>
        /// <param name="e">The event arguments.</param>
        private void PlayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand(_viewModel.PlayCommand, _viewModel.SelectedVideo);
        }

        /// <summary>
        /// Handles the Click event of the Rename MenuItem.
        /// Executes the RenameCommand for the currently selected video file.
        /// </summary>
        /// <param name="sender">The sender of the event (MenuItem).</param>
        /// <param name="e">The event arguments.</param>
        private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand(_viewModel.RenameCommand, _viewModel.SelectedVideo);
        }

        /// <summary>
        /// Handles the Click event of the Delete MenuItem.
        /// Executes the DeleteCommand for the currently selected video file.
        /// </summary>
        /// <param name="sender">The sender of the event (MenuItem).</param>
        /// <param name="e">The event arguments.</param>
        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExecuteCommand(_viewModel.DeleteCommand, _viewModel.SelectedVideo);
        }

        /// <summary>
        /// Executes the specified command with the given parameter, if the command can be executed.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="parameter">The parameter to pass to the command.</param>
        private void ExecuteCommand(ICommand command, object parameter)
        {
            command.Execute(parameter);
        }
        #endregion

        #region Private Fields
        private DispatcherTimer _timer;
        private VideoExplorerViewModel _viewModel;
        #endregion

    }
}
