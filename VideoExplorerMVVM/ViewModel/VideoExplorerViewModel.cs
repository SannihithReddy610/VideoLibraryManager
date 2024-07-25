using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using VideoExplorerMVVM.Model;


namespace VideoExplorerMVVM.ViewModel
{
    public class VideoExplorerViewModel : ViewModelBase
    {

        public VideoExplorerViewModel()
        {
            LoadVideosCommand = new RelayCommand(LoadVideos);
            PlayCommand = new RelayCommand(Play, CanPlay);
            PauseCommand = new RelayCommand(Pause, CanPause);
            StopCommand = new RelayCommand(Stop, CanStop);
            VideoDoubleClickCommand = new RelayCommand<VideoFile>(OnVideoDoubleClick);
            LoadVideos();
        }

        public VideoFile SelectedVideo
        {
            get => _selectedVideo;
            set
            {
                if (_selectedVideo != value)
                {
                    _selectedVideo = value;
                    OnPropertyChanged(nameof(SelectedVideo));
                    UpdateCanExecute();
                }
            }
        }

        public MediaElement MediaElement
        {
            get => _mediaElement;
            set
            {
                _mediaElement = value;
                OnPropertyChanged(nameof(MediaElement));
            }
        }

        public double SeekBarMaximum
        {
            get => seekBarMaximum;
            set => SetProperty(ref seekBarMaximum, value);
        }

        private void LoadVideos()
        {
            Folders.Clear();
            var videoFiles = GetVideoFiles(new List<string> { "C:\\Users", "D:\\", "G:\\" });

            var groupedVideos = videoFiles.GroupBy(v => v.FolderPath);

            foreach (var group in groupedVideos)
            {
                var folderViewModel = new FolderViewModel(group.Key);
                foreach (var video in group)
                {
                    folderViewModel.Videos.Add(video);
                }
                Folders.Add(folderViewModel);
            }
        }

        private IEnumerable<VideoFile> GetVideoFiles(List<string> rootPaths)
        {
            var videoFiles = new List<VideoFile>();
            foreach (var rootPath in rootPaths)
            {
                if (!Directory.Exists(rootPath))
                {
                    Console.WriteLine($"The root path '{rootPath}' does not exist.");
                    return videoFiles;
                }

                // Define video file extensions
                HashSet<string> videoExtensions = new HashSet<string> { ".mp4", ".avi", ".mkv" };

                List<string> allFiles = new List<string>();
                GetFiles(rootPath, allFiles);

                foreach (var file in allFiles)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(file) && videoExtensions.Contains(Path.GetExtension(file).ToLower()))
                        {
                            videoFiles.Add(new VideoFile
                            {
                                FilePath = file,
                                FileName = Path.GetFileName(file),
                                FolderPath = Path.GetDirectoryName(file)
                            });
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            return videoFiles;
        }

        private List<string> GetFiles(string path, List<string> files)
        {
            try
            {
                string[] fileEntries = null;

                fileEntries = Directory.GetFiles(path);

                if (fileEntries != null)
                {
                    foreach (var file in fileEntries)
                    {
                        files.Add(file);
                    }
                }

                string[] directoryEntries = null;
                directoryEntries = Directory.GetDirectories(path);


                if (directoryEntries != null)
                {
                    Parallel.ForEach(directoryEntries, directory =>
                    {
                        GetFiles(directory, files);
                    });
                }
            }
            catch (Exception ex) { }

            return files;
        }

        private void Play()
        {
            if (MediaElement != null)
            {
                if (_isPaused)
                {
                    MediaElement.Play();
                }
                else if (SelectedVideo != null)
                {
                    MediaElement.Source = new Uri(SelectedVideo.FilePath);
                    MediaElement.Play();
                }
                _isPlaying = true;
                _isPaused = false;
                _isStopped = false;
                UpdateCanExecute();
            }
        }

        private void Pause()
        {
            if (MediaElement != null)
            {
                MediaElement.Pause();
                _isPlaying = false;
                _isPaused = true;
                _isStopped = false;
                UpdateCanExecute();
            }
        }

        private void Stop()
        {
            if (MediaElement != null)
            {
                MediaElement.Stop();
                _isPlaying = false;
                _isPaused = false;
                _isStopped = true;
                UpdateCanExecute();
            }
        }

        private bool CanPlay() => !_isPlaying;
        private bool CanPause() => _isPlaying;
        private bool CanStop() => _isPlaying || _isPaused;

        private void OnVideoDoubleClick(VideoFile videoFile)
        {
            SelectedVideo = videoFile;
            Play();
        }

        private void UpdateCanExecute()
        {
            ((RelayCommand)PlayCommand).NotifyCanExecuteChanged();
            ((RelayCommand)PauseCommand).NotifyCanExecuteChanged();
            ((RelayCommand)StopCommand).NotifyCanExecuteChanged();
        }

        private bool _isPlaying;
        private bool _isPaused;
        private bool _isStopped;
        private VideoFile _selectedVideo;
        private MediaElement _mediaElement;
        private double seekBarMaximum;

        public ObservableCollection<FolderViewModel> Folders { get; } = new ObservableCollection<FolderViewModel>();
        public ICommand LoadVideosCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand VideoDoubleClickCommand { get; }

    }
}
