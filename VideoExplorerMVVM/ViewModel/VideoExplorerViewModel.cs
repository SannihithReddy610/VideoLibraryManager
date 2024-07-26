#region Directives
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using VideoExplorerMVVM.Model;
#endregion

namespace VideoExplorerMVVM.ViewModel
{
    public class VideoExplorerViewModel : ViewModelBase
    {
        #region Constructor
        public VideoExplorerViewModel()
        {
            LoadVideosCommand = new RelayCommand(LoadVideos);
            PlayCommand = new RelayCommand(Play, CanPlay);
            PauseCommand = new RelayCommand(Pause, CanPause);
            StopCommand = new RelayCommand(Stop, CanStop);
            VideoDoubleClickCommand = new RelayCommand<VideoFile>(OnVideoDoubleClick);
            LoadVideos();
        }
        #endregion

        #region Properties
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

        public double SeekBarValue
        {
            get => _seekBarValue;
            set
            {
                if (_seekBarValue != value)
                {
                    _seekBarValue = value;
                    OnPropertyChanged(nameof(SeekBarValue));
                }
            }
        }

        public string VideoDuration
        {
            get => _videoDuration;
            set
            {
                if (_videoDuration != value)
                {
                    _videoDuration = value;
                    OnPropertyChanged(nameof(VideoDuration));
                }
            }
        }

        public ObservableCollection<FolderViewModel> Folders { get; } = new ObservableCollection<FolderViewModel>();
        public ICommand LoadVideosCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand VideoDoubleClickCommand { get; }
        #endregion

        #region Private Methods
        /// <summary>
        /// The LoadVideos method clears the existing folder list, retrieves video files 
        /// from specified directories, groups them by folder, 
        /// and updates the Folders collection with FolderViewModel instances for each group.
        /// </summary>
        private void LoadVideos()
        {
            Folders.Clear();
            var videoFiles = GetVideoFiles(["C:\\Users", "D:\\", "G:\\"]);

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

        /// <summary>
        /// The GetVideoFiles method retrieves video files from a list of root paths, 
        /// filtering by specific file extensions
        /// </summary>
        /// <param name="rootPaths"></param>
        /// <returns></returns>
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

        /// <summary>
        /// GetFiles method recursively collects all file paths within a given directory 
        /// and its subdirectories into a provided list.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="files"></param>
        private void GetFiles(string path, List<string> files)
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
        }

        /// <summary>
        /// Play method plays the selected video in the MediaElement control if present, 
        /// updates playback state variables.
        /// </summary>
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

        /// <summary>
        /// Pause method pauses the currently playing video in the MediaElement control 
        /// and updates playback state variables.
        /// </summary>
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

        /// <summary>
        /// Stop method stops the currently playing video in the MediaElement control 
        /// and updates playback state variables.
        /// </summary>
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

        /// <summary>
        /// Returns true if the media is not currently playing.
        /// </summary>
        private bool CanPlay() => !_isPlaying;

        /// <summary>
        /// Returns true if the media is currently playing.
        /// </summary>
        private bool CanPause() => _isPlaying;

        /// <summary>
        /// Returns true if the media is either playing or paused.
        /// </summary>
        private bool CanStop() => _isPlaying || _isPaused;

        /// <summary>
        /// On double clicking the video, this method sets the selected video and starts playback.
        /// </summary>
        /// <param name="videoFile"></param>
        private void OnVideoDoubleClick(VideoFile videoFile)
        {
            SelectedVideo = videoFile;
            Play();
        }

        /// <summary>
        /// Notifies play, pause and stop commands about potential changes in their execution state.
        /// </summary>
        private void UpdateCanExecute()
        {
            ((RelayCommand)PlayCommand).NotifyCanExecuteChanged();
            ((RelayCommand)PauseCommand).NotifyCanExecuteChanged();
            ((RelayCommand)StopCommand).NotifyCanExecuteChanged();
        }
        #endregion

        #region Private Fields
        private bool _isPlaying;
        private bool _isPaused;
        private bool _isStopped;
        private VideoFile _selectedVideo;
        private MediaElement _mediaElement;
        private double _seekBarValue;
        private string _videoDuration;
        #endregion

    }
}
