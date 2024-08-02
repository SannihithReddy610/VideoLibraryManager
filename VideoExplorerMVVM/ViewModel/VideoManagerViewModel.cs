#region Directives
using CommunityToolkit.Mvvm.Input;
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
using VideoLibraryManager.Helper;

#endregion

namespace VideoLibraryManager.ViewModel
{
    public class VideoManagerViewModel : ViewModelBase
    {
        #region Constructor
        public VideoManagerViewModel(ILogger<VideoManagerViewModel> logger) : this(new CloudVideoFileManagementService(), new LocalVideoFileManagementService(logger), new VideoPlayerService(), logger)
        {

        }

        public VideoManagerViewModel(ICloudVideoFileManagementService cloudVideoFileManagementService, ILocalVideoFileManagementService localVideoFileManagementService, IVideoPlayerService videoPlayerService, ILogger logger)
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

            _localVideoFileManagementService = localVideoFileManagementService;
            _cloudVideoFileManagementService = cloudVideoFileManagementService;
            _videoPlayerService = videoPlayerService;
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

        public Visibility FolderListVisibility => IsFullScreen ? Visibility.Collapsed : Visibility.Visible;

        #region Commands
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
        #endregion

        #endregion

        #region Private Methods
        #region Video Loading and Filtering
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
                await Current.Dispatcher.InvokeAsync(() =>
                {
                    FilteredFolders = new ObservableCollection<FolderViewModel>(filteredFolders);
                });
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }
        #endregion

        #region VideoPlayerService
        /// <summary>
        /// Plays the selected video in the MediaElement control if present, updates playback state variables.
        /// </summary>
        private void PlayVideo()
        {
            try
            {
                if (SelectedVideo != null)
                {
                    _videoPlayerService.PlayVideo(IsPaused, MediaElement, SelectedVideo.FilePath);
                    PlayingVideo = SelectedVideo.FileName;
                    IsPaused = false;
                    IsPlaying = true;
                    UpdateCanExecute();
                }
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
            SelectedVideo = videoFile;
            PlayVideo();
        }

        /// <summary>
        /// Pauses the currently playing video in the MediaElement control and updates playback state variables.
        /// </summary>
        private void Pause()
        {
            try
            {
                _videoPlayerService.Pause(MediaElement);
                IsPaused = true;
                IsPlaying = false;
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
                _videoPlayerService.Stop(MediaElement);
                IsPlaying = false;
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
            if (IsPlaying)
            {
                IsFullScreen = !IsFullScreen;
                MediaElement.Play();
            }
            else
            {
                IsFullScreen = !IsFullScreen;
                PlayVideo();
            }
        }
        #endregion

        #region LocalVideoFileManagementService
        /// <summary>
        /// Loads video files asynchronously from specified directories, groups them by folder, 
        /// and updates the Folders collection.
        /// </summary>
        private async Task LoadVideosAsync()
        {
            StatusMessage = "Loading videos...";
            try
            {
                var videoFiles = await _localVideoFileManagementService.LoadVideosAsync();

                await Current.Dispatcher.InvokeAsync(() =>
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
        /// Renames the selected video file.
        /// </summary>
        private void RenameVideo()
        {
            try
            {
                string newFileName = Microsoft.VisualBasic.Interaction.InputBox("Enter the new name for the video:", "Rename Video", SelectedVideo.FileName);
                var newFilePath = _localVideoFileManagementService.RenameVideo(SelectedVideo.FileName, SelectedVideo.FilePath, newFileName);
                if (newFilePath != string.Empty)
                {
                    SelectedVideo.FilePath = newFilePath;
                    SelectedVideo.FileName = newFileName;
                    var folder = Folders.FirstOrDefault(f => f.Videos.Contains(SelectedVideo));
                    if (folder != null)
                    {
                        var video = folder.Videos.FirstOrDefault(v => v.FilePath == newFilePath);
                        if (video != null)
                        {
                            video.FileName = newFileName;
                        }
                    }
                    OnPropertyChanged(nameof(FileName));
                }
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
                MessageBoxResult result = Show($"Are you sure you want to permanently delete '{SelectedVideo.FileName}'?",
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    StatusMessage = "Deleting Video. Status will be notified";
                    _localVideoFileManagementService.DeleteVideo(SelectedVideo.FilePath);
                    var folder = Folders.FirstOrDefault(f => f.Videos.Contains(SelectedVideo));
                    if (folder != null)
                    {
                        folder.Videos.Remove(SelectedVideo);
                        if (!folder.Videos.Any())
                        {
                            Folders.Remove(folder);
                        }
                    }

                    // Clear the selected video
                    SelectedVideo = null;
                    OnPropertyChanged(nameof(FileName));
                    StatusMessage = "Video deleted successfully.";
                }

            }
            catch (Exception ex)
            {
                Show($"An error occurred while deleting the video: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region CloudVideoFileManagementService
        /// <summary>
        /// Loads the list of video files available in the cloud and updates the UI.
        /// </summary>
        private async Task LoadCloudVideosAsync()
        {
            try
            {
                var response = await _cloudVideoFileManagementService.LoadCloudVideosAsync();

                await Current.Dispatcher.InvokeAsync(() =>
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
        /// Uploads selected video file to the cloud.
        /// </summary>
        private async Task UploadVideo()
        {
            try
            {
                StatusMessage = "Uploading Video. Status will be notified";
                var response = await _cloudVideoFileManagementService.UploadVideo(SelectedVideo.FilePath);
                if (response.IsSuccessStatusCode)
                {
                    await LoadCloudVideosAsync();
                    StatusMessage = "";
                    Show($"{Path.GetFileName(SelectedVideo.FilePath)} uploaded successfully.");
                }
                else
                {
                    Show($"File upload failed. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }

        /// <summary>
        /// Upload a new version of the video which is on cloud
        /// </summary>
        private async Task UploadNewVersionOfVideo()
        {
            try
            {
                StatusMessage = "Uploading new version. You will be notified";
                await _cloudVideoFileManagementService.UploadNewVersionOfVideo(CloudSelectedVideo.FileName);
                StatusMessage = "";
                Show("New version uploaded successfully");
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
                await _cloudVideoFileManagementService.DownloadPreviousVersionOfVideo(CloudSelectedVideo.FileName);
                await LoadVideosAsync();
                StatusMessage = "Video Downloaded successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File not found");
                Show("No old version available for the selected video", ex.Message);
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
                await _cloudVideoFileManagementService.DownloadVideo(CloudSelectedVideo.FileName);
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
                await _cloudVideoFileManagementService.DeleteCloudFileAsync(CloudSelectedVideo.FileName);
                await LoadCloudVideosAsync();
                StatusMessage = "Video Deleted Successfully.";
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }
        #endregion

        #region Helper Methods
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
        #endregion
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
        private readonly ILogger _logger;
        private readonly ILocalVideoFileManagementService _localVideoFileManagementService;
        private readonly IVideoPlayerService _videoPlayerService;
        private readonly ICloudVideoFileManagementService _cloudVideoFileManagementService;
        //private readonly IDirectoryHelper _directoryHelper;
        #endregion

    }
}
