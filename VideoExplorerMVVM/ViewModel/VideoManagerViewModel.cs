#region Directives
using CommunityToolkit.Mvvm.Input;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VideoLibraryManager.Model;
using static System.Windows.Application;
using static System.Windows.MessageBox;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using VideoLibraryManager.Services;

#endregion

namespace VideoLibraryManager.ViewModel
{
    public class VideoManagerViewModel : ViewModelBase
    {
        #region Constructor
        public VideoManagerViewModel(ILogger<VideoManagerViewModel> logger)
        {
            _logger = logger;

            LoadVideosCommand = new AsyncRelayCommand(LoadVideosAsync);
            SyncVideosCommand = new AsyncRelayCommand(SyncVideosAsync);
            LoadCloudVideosCommand = new AsyncRelayCommand(LoadCloudVideosAsync);
            DeleteCloudFileCommand = new AsyncRelayCommand(DeleteCloudFileAsync);
            UploadVideoCommand = new AsyncRelayCommand(UploadVideo);
            UploadNewVersionCommand = new AsyncRelayCommand(UploadNewVersionOfVideo);
            DownloadPreviousVersionCommand = new AsyncRelayCommand(DownloadPreviousVersionOfVideo);
            DownloadFileCommand = new AsyncRelayCommand(DownloadVideo);
            PlayCommand = new RelayCommand(PlayVideo);
            PauseCommand = new RelayCommand(Pause);
            StopCommand = new RelayCommand(Stop);
            RenameCommand = new RelayCommand(RenameVideo);
            DeleteCommand = new RelayCommand(DeleteVideo);
            VideoDoubleClickCommand = new RelayCommand<VideoFile>(PlayVideoOnDoubleClick);
            ToggleFullScreenCommand = new RelayCommand(ToggleFullScreen);

            Folders = new ObservableCollection<FolderViewModel>();
            FilteredFolders = new ObservableCollection<FolderViewModel>();
            CloudVideos = new ObservableCollection<CloudVideoFile>();

            _artifactoryKey = Environment.GetEnvironmentVariable("JFROG_API_KEY");
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Add("X-JFrog-Art-Api", _artifactoryKey);

            LoadInputConfiguration = LoadConfiguration();
            _rootPaths = LoadInputConfiguration.GetSection("RootPaths").Get<List<string>>();
            _videoExtensions = new HashSet<string>(LoadInputConfiguration.GetSection("VideoExtensions").Get<List<string>>());
            _artifactoryUrl = LoadInputConfiguration["ArtifactoryUrl"];

            _localVideoFileManagementService = new LocalVideoFileManagementService(this);
            _videoPlayerService = new VideoPlayerService(this);
            _cloudVideoFileManagementService = new CloudVideoFileManagementService(this);
        }
        #endregion

        #region Properties
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
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
                }
            }
        }

        public HttpClient HttpClient { get; set; }

        public CloudVideoFile CloudSelectedVideo
        {
            get => _cloudSelectedVideo;
            set
            {
                if (_cloudSelectedVideo != value)
                {
                    _cloudSelectedVideo = value;
                    OnPropertyChanged(nameof(CloudSelectedVideo));
                }
            }
        }

        public string PlayingVideo
        {
            get => _playingVideo;
            set
            {
                _playingVideo = value;
                OnPropertyChanged(nameof(PlayingVideo));
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

        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }

        public IConfiguration LoadInputConfiguration { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    _ = FilterVideosAsync();
                }
            }
        }

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
                if (_isFullScreen != value)
                {
                    _isFullScreen = value;
                    OnPropertyChanged(nameof(IsFullScreen));
                    OnPropertyChanged(nameof(FolderListVisibility));
                }
            }
        }

        public bool IsPlaying { get; set; }

        public bool IsPaused { get; set; }

        public ObservableCollection<FolderViewModel> FilteredFolders
        {
            get => _filteredFolders;
            set
            {
                _filteredFolders = value;
                OnPropertyChanged(nameof(FilteredFolders));
            }
        }

        public ObservableCollection<FolderViewModel> Folders { get; set; }

        public ObservableCollection<CloudVideoFile> CloudVideos { get; }

        public IAsyncRelayCommand LoadVideosCommand { get; }

        public IAsyncRelayCommand LoadCloudVideosCommand { get; }

        public IAsyncRelayCommand DeleteCloudFileCommand { get; }

        public ICommand SyncVideosCommand { get; }

        public ICommand PlayCommand { get; }

        public ICommand PauseCommand { get; }

        public ICommand StopCommand { get; }

        public ICommand RenameCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand VideoDoubleClickCommand { get; }

        public ICommand ToggleFullScreenCommand { get; }

        public ICommand UploadVideoCommand { get; }

        public ICommand UploadNewVersionCommand { get; }

        public ICommand DownloadPreviousVersionCommand { get; }

        public ICommand DownloadFileCommand { get; }

        public Visibility FolderListVisibility => IsFullScreen ? Visibility.Collapsed : Visibility.Visible;
        #endregion

        #region Private Methods
        /// <summary>
        /// Loads video files asynchronously from specified directories, groups them by folder, 
        /// and updates the Folders collection.
        /// </summary>
        private async Task LoadVideosAsync()
        {
            StatusMessage = "Loading videos...";
            try
            {
                var videoFiles = await GetVideoFilesAsync(_rootPaths).ConfigureAwait(false);

                Current.Dispatcher.Invoke(() =>
                {
                    Folders.Clear();
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
                    _ = FilterVideosAsync();
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred while loading local videos: {ex.Message}";
                _logger.LogError(ex, "An error occurred while loading local videos");
            }
            finally
            {
                StatusMessage = "Local Videos loaded successfully.";
            }
        }

        /// <summary>
        /// Loads the list of video files available in the cloud and updates the UI.
        /// </summary>
        private async Task LoadCloudVideosAsync()
        {
            try
            {
                var response = await HttpClient.GetStringAsync(_artifactoryUrl);

                Current.Dispatcher.Invoke(() =>
                {
                    var fileList = ExtractFileNames(response);

                    CloudVideos.Clear();
                    foreach (var file in fileList)
                    {
                        CloudVideos.Add(file);
                    }
                    StatusMessage = "Cloud Videos loaded successfully.";
                });
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }

        /// <summary>
        /// Synchronizes the videos by clearing the current folders and reloading the videos.
        /// </summary>
        private async Task SyncVideosAsync()
        {
            try
            {
                _ = LoadVideosAsync();
                _ = LoadCloudVideosAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred while syncing videos: {ex.Message}";
                _logger.LogError(ex, $"An error occurred while syncing videos: {ex.Message}");
            }
            finally
            {
                StatusMessage = "Syncing videos, please wait...";
            }
        }

        /// <summary>
        /// Asynchronously filters the video list based on the search text.
        /// If the search text is empty or whitespace, all videos are shown.
        /// Otherwise, videos whose filenames contain the search text are displayed.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task FilterVideosAsync()
        {
            try
            {
                var searchText = _searchText?.ToLower().Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    // Show all videos if search text is empty
                    FilteredFolders = new ObservableCollection<FolderViewModel>(Folders);
                    return;
                }

                // Perform the filtering in a background task
                var filteredFolders = await Task.Run(() =>
                {
                    var result = new List<FolderViewModel>();

                    foreach (var folder in Folders)
                    {
                        var filteredVideos = folder.Videos
                            .Where(v => v.FileName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                            .ToList();

                        if (filteredVideos.Count > 0)
                        {
                            var filteredFolder = new FolderViewModel(folder.FolderPath);
                            foreach (var video in filteredVideos)
                            {
                                filteredFolder.Videos.Add(video);
                            }
                            result.Add(filteredFolder);
                        }
                    }

                    return result;
                });

                // Update the FilteredFolders collection on the UI thread
                Current.Dispatcher.Invoke(() =>
                {
                    FilteredFolders = new ObservableCollection<FolderViewModel>(filteredFolders);
                });
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }
        #region VideoPlayerService

        /// <summary>
        /// Plays the selected video in the MediaElement control if present, updates playback state variables.
        /// </summary>
        private void PlayVideo()
        {
            try
            {
                _videoPlayerService.PlayVideo();
                UpdateCanExecute();
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }

        /// <summary>
        /// Handles double-click on a video to set the selected video and start playback.
        /// </summary>
        /// <param name="videoFile">The video file to play.</param>
        private void PlayVideoOnDoubleClick(VideoFile videoFile)
        {
            _videoPlayerService.PlayVideoOnDoubleClick(videoFile);
        }

        /// <summary>
        /// Pauses the currently playing video in the MediaElement control and updates playback state variables.
        /// </summary>
        private void Pause()
        {
            try
            {
                _videoPlayerService.Pause();
                UpdateCanExecute();
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }

        /// <summary>
        /// Stops the currently playing video in the MediaElement control and updates playback state variables.
        /// </summary>
        private void Stop()
        {
            try
            {
                _videoPlayerService.Stop();
                UpdateCanExecute();
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }

        /// <summary>
        /// Toggles the full-screen mode of the application.
        /// </summary>
        private void ToggleFullScreen()
        {
            _videoPlayerService.ToggleFullScreen();
        }
        #endregion

        #region LocalVideoFileManagementService
        /// <summary>
        /// Renames the selected video file.
        /// </summary>
        private void RenameVideo()
        {
            try
            {
                _localVideoFileManagementService.RenameVideo();
                OnPropertyChanged(nameof(FileName));
            }
            catch (Exception ex)
            {
                Show($"An error occurred while renaming the video: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Deletes the selected video file.
        /// </summary>
        private void DeleteVideo()
        {
            try
            {
                _localVideoFileManagementService.DeleteVideo();
                OnPropertyChanged(nameof(FileName));
                _ = LoadVideosAsync();
            }
            catch (Exception ex)
            {
                Show($"An error occurred while deleting the video: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Uploads selected video file to the cloud.
        /// </summary>
        private async Task UploadVideo()
        {
            try
            {
                StatusMessage = "Uploading Video. Status will be notified";
                await _localVideoFileManagementService.UploadVideo();
                await LoadCloudVideosAsync();
                StatusMessage = "Video Uploaded Successfully";
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }
        #endregion

        #region CloudVideoFileManagementService
        /// <summary>
        /// Upload a new version of the video which is on cloud
        /// </summary>
        private async Task UploadNewVersionOfVideo()
        {
            try
            {
                StatusMessage = "Uploading new version. You will be notified";
                await _cloudVideoFileManagementService.UploadNewVersionOfVideo();
                StatusMessage = "";
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }

        /// <summary>
        /// Downloads the previous version video file for the currently selected video file on cloud
        /// </summary>
        private async Task DownloadPreviousVersionOfVideo()
        {
            try
            {
                StatusMessage = "Downloading Video. Status will be notified";
                await _cloudVideoFileManagementService.DownloadPreviousVersionOfVideo();
                await LoadVideosAsync();
                StatusMessage = "Video Downloaded successfully.";
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }

        /// <summary>
        /// Download selected video file from cloud to local.
        /// </summary>
        private async Task DownloadVideo()
        {
            try
            {
                StatusMessage = "Downloading Video. Status will be notified";
                await _cloudVideoFileManagementService.DownloadVideo();
                await LoadVideosAsync();
                StatusMessage = "Downloaded Video Successfully";

            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }

        /// <summary>
        /// Deletes selected video from cloud
        /// </summary>
        private async Task DeleteCloudFileAsync()
        {
            try
            {
                StatusMessage = "Deleting Video. Status will be notified";
                _cloudVideoFileManagementService.DeleteCloudFileAsync();
                await LoadCloudVideosAsync();
                StatusMessage = "Video Deleted Successfully.";
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }
        #endregion

        /// <summary>
        /// Retrieves video files asynchronously from a list of root paths, filtering by specific file extensions.
        /// </summary>
        /// <param name="rootPaths">The list of root paths to search for video files.</param>
        /// <returns>An enumerable collection of video files.</returns>
        private async Task<IEnumerable<VideoFile>> GetVideoFilesAsync(List<string>? rootPaths)
        {
            var videoFiles = new ConcurrentBag<VideoFile>();

            var tasks = rootPaths.Select(async rootPath =>
            {
                if (Directory.Exists(rootPath))
                {
                    var allFiles = new List<string>();
                    await Task.Run(() => GetFiles(rootPath, allFiles));

                    foreach (var file in allFiles)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(file) && _videoExtensions.Contains(Path.GetExtension(file).ToLower()))
                            {
                                videoFiles.Add(new VideoFile(file)
                                {
                                    FilePath = file,
                                    FileName = Path.GetFileName(file),
                                    FolderPath = Path.GetDirectoryName(file)
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error processing file '{file}': {ex.Message}");
                        }
                    }
                }
                else
                {
                    _logger.LogError($"The root path '{rootPath}' does not exist.");
                }
            });

            await Task.WhenAll(tasks);
            return videoFiles;
        }

        /// <summary>
        /// Recursively collects all file paths within a given directory and its subdirectories into a provided list.
        /// </summary>
        /// <param name="path">The root directory path to search.</param>
        /// <param name="files">The list to collect file paths.</param>
        private void GetFiles(string path, List<string> files)
        {
            try
            {
                var fileEntries = Directory.GetFiles(path);
                lock (files)
                {
                    files.AddRange(fileEntries);
                }

                var directoryEntries = Directory.GetDirectories(path);
                Parallel.ForEach(directoryEntries, directory =>
                {
                    GetFiles(directory, files);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error accessing path '{path}': {ex.Message}");
            }
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

        /// <summary>
        /// Extracts the file names from the given HTML content.
        /// </summary>
        /// <param name="htmlContent">The HTML content to extract file names from.</param>
        private IEnumerable<CloudVideoFile> ExtractFileNames(string htmlContent)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(htmlContent, @"href=""([^""]*)""");
            var cloudFileNames = new List<CloudVideoFile>();
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var fileName = match.Groups[1].Value;
                if (!fileName.EndsWith("/"))
                {
                    cloudFileNames.Add(new CloudVideoFile(fileName)
                    {
                        FileName = fileName
                    });
                }
            }
            return cloudFileNames;
        }

        /// <summary>
        /// Loads configuration to read data from Json file
        /// </summary>
        private IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }
        #endregion

        #region Private Fields

        private bool _isFullScreen;
        private double _seekBarValue;
        private string _videoDuration;
        private string _fileName;
        private string _statusMessage;
        private string _searchText;
        private string _playingVideo;
        private VideoFile _selectedVideo;
        private CloudVideoFile _cloudSelectedVideo;
        private MediaElement _mediaElement;
        private ObservableCollection<FolderViewModel> _filteredFolders;
        private readonly List<string>? _rootPaths;
        private readonly HashSet<string> _videoExtensions;
        private readonly string? _artifactoryUrl;
        private readonly string? _artifactoryKey;
        private readonly ILogger<VideoManagerViewModel> _logger;
        private readonly ILocalVideoFileManagementService _localVideoFileManagementService;
        private readonly IVideoPlayerService _videoPlayerService;
        private readonly ICloudVideoFileManagementService _cloudVideoFileManagementService;

        #endregion

    }
}
