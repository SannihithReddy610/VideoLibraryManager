#region Directives
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VideoExplorerMVVM.Model;
using VideoExplorerMVVM.ViewModel;
#endregion

namespace VideoExplorerMVVM
{
    public partial class MainWindow : Window
    {

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            var viewModel = DataContext as VideoExplorerViewModel;
            if (viewModel != null)
            {
                viewModel.MediaElement = mediaElement;
            }

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Tick += Timer_Tick;

            DataContextChanged += MainWindow_DataContextChanged;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// This event handler responds to the changes in the DataContext of the MainWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// This event handler helps to update the ViewModel whenever the selected item changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var viewModel = DataContext as VideoExplorerViewModel;
            if (viewModel != null)
            {
                var selectedVideo = e.NewValue as VideoFile;
                if (selectedVideo != null)
                {
                    viewModel.SelectedVideo = selectedVideo;
                }
            }
        }

        /// <summary>
        /// This is event handler for the PropertyChanged event that helps to notifies the view when 
        /// a property value changes in the ViewModel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VideoExplorerViewModel.SelectedVideo))
            {
                var viewModel = (VideoExplorerViewModel)sender;
                PlayVideo(viewModel.SelectedVideo);
            }
        }

        /// <summary>
        /// This methods handles playback of video 
        /// </summary>
        /// <param name="video"></param>
        private void PlayVideo(VideoFile video)
        {
            if (video != null)
            {
                mediaElement.Source = new Uri(video.FilePath);
                mediaElement.Play();
                timer.Start();
            }
        }

        /// <summary>
        /// This event handler is for MediaOpened event, which is triggered when the media file has 
        /// been successfully opened and is ready for playback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                seekBar.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                timer.Start();
            }
        }

        /// <summary>
        /// This event handler is for MediaEnded event, which is triggered when media playback 
        /// reaches the end of the media file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as VideoExplorerViewModel;
            viewModel?.StopCommand.Execute(null);
            timer.Stop();
        }

        /// <summary>
        /// This event handler is for ValueChanged event of a seek bar control, which handles 
        /// changes to the seek bar's value and updates the playback position of the media accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaElement != null && Math.Abs(mediaElement.Position.TotalSeconds - seekBar.Value) > 1)
            {
                mediaElement.Position = TimeSpan.FromSeconds(seekBar.Value);
            }
        }

        /// <summary>
        /// This event handler is for ValueChanged event of a volume slider control, which updates 
        /// the volume of the MediaElement control based on the slider’s value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaElement != null)
            {
                mediaElement.Volume = volumeSlider.Value;
            }
        }

        /// <summary>
        /// This event handler is executed on each tick of a timer, which helps 
        /// to update the value of a seek bar control to reflect the current playback position of the media
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaElement != null && mediaElement.NaturalDuration.HasTimeSpan)
            {
                // Ensure this update is performed on the UI thread
                Dispatcher.Invoke(() =>
                {
                    seekBar.Value = mediaElement.Position.TotalSeconds;
                });
            }
        }
        #endregion

        #region Private Fields
        private DispatcherTimer timer;
        #endregion
    }
}
