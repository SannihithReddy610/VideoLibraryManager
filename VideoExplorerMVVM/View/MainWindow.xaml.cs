using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VideoExplorerMVVM.Model;
using VideoExplorerMVVM.ViewModel;

namespace VideoExplorerMVVM
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;

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
                Interval = TimeSpan.FromMilliseconds(100) // Shorter interval for more frequent updates
            };
            timer.Tick += Timer_Tick;

            DataContextChanged += MainWindow_DataContextChanged;
        }

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

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VideoExplorerViewModel.SelectedVideo))
            {
                var viewModel = (VideoExplorerViewModel)sender;
                PlayVideo(viewModel.SelectedVideo);
            }
        }

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

        private void PlayVideo(VideoFile video)
        {
            if (video != null)
            {
                mediaElement.Source = new Uri(video.FilePath);
                mediaElement.Play();
                timer.Start();
            }
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                seekBar.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                timer.Start();
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as VideoExplorerViewModel;
            viewModel?.StopCommand.Execute(null);
            timer.Stop();
        }

        private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaElement != null && Math.Abs(mediaElement.Position.TotalSeconds - seekBar.Value) > 1)
            {
                mediaElement.Position = TimeSpan.FromSeconds(seekBar.Value);
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaElement != null)
            {
                mediaElement.Volume = volumeSlider.Value;
            }
        }

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
    }
}
