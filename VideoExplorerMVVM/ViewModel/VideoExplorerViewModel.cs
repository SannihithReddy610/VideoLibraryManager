﻿#region Directives
using CommunityToolkit.Mvvm.Input;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VideoExplorerMVVM.Model;
using static System.Windows.MessageBox;
#endregion

namespace VideoExplorerMVVM.ViewModel
{
    public class VideoExplorerViewModel : ViewModelBase
    {
        #region Constructor
        public VideoExplorerViewModel()
        {
            LoadVideosCommand = new AsyncRelayCommand(LoadVideosAsync);
            SyncVideosCommand = new AsyncRelayCommand(SyncVideosAsync);
            LoadCloudVideosCommand = new AsyncRelayCommand(LoadCloudVideosAsync);
            PlayCommand = new RelayCommand(PlayVideo, CanPlay);
            PauseCommand = new RelayCommand(Pause, CanPause);
            StopCommand = new RelayCommand(Stop, CanStop);
            RenameCommand = new RelayCommand(RenameVideo, CanRename);
            DeleteCommand = new RelayCommand(DeleteVideo, CanDelete);
            UploadVideoCommand = new RelayCommand(UploadVideo);
            VideoDoubleClickCommand = new RelayCommand<VideoFile>(PlayVideoOnDoubleClick);
            ToggleFullScreenCommand = new RelayCommand(ToggleFullScreen);
            Folders = new ObservableCollection<FolderViewModel>();
            FilteredFolders = new ObservableCollection<FolderViewModel>();
            CloudVideos = new ObservableCollection<string>();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-JFrog-Art-Api", apiKey);
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
                    UpdateCanExecute();
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
                    UpdateCanExecute();
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
                    _= FilterVideosAsync();
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

        public ObservableCollection<string> CloudVideos { get; }

        public IAsyncRelayCommand LoadVideosCommand { get; set; }

        public IAsyncRelayCommand SyncVideosCommand { get; set; }

        public IAsyncRelayCommand LoadCloudVideosCommand { get; }

        public ICommand PlayCommand { get; }

        public ICommand PauseCommand { get; }

        public ICommand StopCommand { get; }

        public ICommand RenameCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand VideoDoubleClickCommand { get; }

        public ICommand ToggleFullScreenCommand { get; }

        public ICommand UploadVideoCommand { get;  }

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
                var videoFiles = await GetVideoFilesAsync(RootPaths).ConfigureAwait(false);

                Application.Current.Dispatcher.Invoke(() =>
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
                StatusMessage = $"An error occurred while loading videos: {ex.Message}";
                // Log the exception or handle it as needed
                Console.WriteLine($"An error occurred while loading videos: {ex.Message}");
            }
            finally
            {
                StatusMessage = "Local Videos loaded successfully.";
            }
        }

        /// <summary>
        /// Retrieves video files asynchronously from a list of root paths, filtering by specific file extensions.
        /// </summary>
        /// <param name="rootPaths">The list of root paths to search for video files.</param>
        /// <returns>An enumerable collection of video files.</returns>
        private async Task<IEnumerable<VideoFile>> GetVideoFilesAsync(List<string> rootPaths)
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
                            if (!string.IsNullOrEmpty(file) && VideoExtensions.Contains(Path.GetExtension(file).ToLower()))
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
                            Console.WriteLine($"Error processing file '{file}': {ex.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"The root path '{rootPath}' does not exist.");
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
                if (fileEntries != null)
                {
                    lock (files)
                    {
                        files.AddRange(fileEntries);
                    }
                }

                var directoryEntries = Directory.GetDirectories(path);
                if (directoryEntries != null)
                {
                    Parallel.ForEach(directoryEntries, directory =>
                    {
                        GetFiles(directory, files);
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing path '{path}': {ex.Message}");
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
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred while syncing videos: {ex.Message}";
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                FilteredFolders = new ObservableCollection<FolderViewModel>(filteredFolders);
            });
        }

        /// <summary>
        /// Plays the selected video in the MediaElement control if present, updates playback state variables.
        /// </summary>
        private void PlayVideo()
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
                    PlayingVideo = SelectedVideo.FileName;
                }
                _isPlaying = true;
                _isPaused = false;
                _isStopped = false;
                UpdateCanExecute();
            }
        }

        /// <summary>
        /// Pauses the currently playing video in the MediaElement control and updates playback state variables.
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
        /// Stops the currently playing video in the MediaElement control and updates playback state variables.
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
        /// Renames the selected video file.
        /// </summary>
        private void RenameVideo()
        {
            if (SelectedVideo == null)
                return;

            try
            {
                // Ask the user for a new file name
                string newFileName = Microsoft.VisualBasic.Interaction.InputBox("Enter the new name for the video:", "Rename Video", SelectedVideo.FileName);

                if (string.IsNullOrWhiteSpace(newFileName) || newFileName == SelectedVideo.FileName)
                    return;

                string newFilePath = Path.Combine(Path.GetDirectoryName(SelectedVideo.FilePath), newFileName);

                // Check if the new file name already exists
                if (File.Exists(newFilePath))
                {
                    Show("A file with the new name already exists. Please choose a different name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Rename the file
                File.Move(SelectedVideo.FilePath, newFilePath);

                // Update the view model
                SelectedVideo.FilePath = newFilePath;
                SelectedVideo.FileName = newFileName;
                OnPropertyChanged(nameof(FileName));
                var folder = Folders.FirstOrDefault(f => f.Videos.Contains(SelectedVideo));
                if (folder != null)
                {
                    var video = folder.Videos.FirstOrDefault(v => v.FilePath == newFilePath);
                    if (video != null)
                    {
                        video.FileName = newFileName;
                    }
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
            if (SelectedVideo == null)
                return;

            try
            {
                // Confirm the deletion action with the user
                MessageBoxResult result = Show($"Are you sure you want to permanently delete '{SelectedVideo.FileName}'?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    // Delete the video file from the file system
                    if (File.Exists(SelectedVideo.FilePath))
                    {
                        File.Delete(SelectedVideo.FilePath);
                    }

                    // Remove the video file from the view model
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
                }
            }
            catch (Exception ex)
            {
                Show($"An error occurred while deleting the video: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Toggles the full-screen mode of the application.
        /// </summary>
        private void ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;
        }

        /// <summary>
        /// Determines if the Play command can execute.
        /// </summary>
        private bool CanPlay() => !_isPlaying;

        /// <summary>
        /// Determines if the Pause command can execute.
        /// </summary>
        private bool CanPause() => _isPlaying;

        /// <summary>
        /// Determines if the Stop command can execute.
        /// </summary>
        private bool CanStop() => _isPlaying || _isPaused;

        /// <summary>
        /// Determines if the Delete command can execute.
        /// </summary>
        private bool CanDelete() => SelectedVideo != null;

        /// <summary>
        /// Determines if the Rename command can execute.
        /// </summary>
        private bool CanRename() => SelectedVideo != null;

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
        /// Notifies play, pause, stop, rename, and delete commands about potential changes in their execution state.
        /// </summary>
        private void UpdateCanExecute()
        {
            ((RelayCommand)PlayCommand).NotifyCanExecuteChanged();
            ((RelayCommand)PauseCommand).NotifyCanExecuteChanged();
            ((RelayCommand)StopCommand).NotifyCanExecuteChanged();
            ((RelayCommand)RenameCommand).NotifyCanExecuteChanged();
            ((RelayCommand)DeleteCommand).NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Opens a file dialog to select a video file and uploads it to the cloud.
        /// </summary>
        private async void UploadVideo()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Video Files (*.mp4;*.avi;*.mkv)|*.mp4;*.avi;*.mkv",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                await UploadFileToCloud(filePath);
            }
        }

        /// <summary>
        /// Uploads a video file to the cloud.
        /// </summary>
        /// <param name="filePath">The path of the file to be uploaded.</param>
        private async Task UploadFileToCloud(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            StatusMessage = " Uploading Video. Status will be notified";
            using (_httpClient)
            {
                using (var content = new MultipartFormDataContent())
                {
                    byte[] fileBytes = File.ReadAllBytes(filePath);
                    var fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    content.Add(fileContent, "file", Path.GetFileName(filePath));
                    var response = await _httpClient.PutAsync(artifactoryUrl + fileName, content);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Show(response.IsSuccessStatusCode
                            ? $"{fileName} uploaded successfully."
                            : $"File upload failed. Status code: {response.StatusCode}");
                    });
                    StatusMessage = "";
                }
            }
        }

        /// <summary>
        /// Loads the list of video files available in the cloud and updates the UI.
        /// </summary>
        private async Task LoadCloudVideosAsync()
        {
            var response = await _httpClient.GetStringAsync(artifactoryUrl);

            Application.Current.Dispatcher.Invoke(() =>
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

        /// <summary>
        /// Extracts the file names from the given HTML content.
        /// </summary>
        /// <param name="htmlContent">The HTML content to extract file names from.</param>
        private string[] ExtractFileNames(string htmlContent)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(htmlContent, @"href=""([^""]*)""");
            var fileNames = new List<string>();
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var fileName = match.Groups[1].Value;
                if (!fileName.EndsWith("/"))
                {
                    fileNames.Add(fileName);
                }
            }
            return fileNames.ToArray();
        }
        #endregion

        #region Private Fields
        private bool _isPlaying;
        private bool _isPaused;
        private bool _isStopped;
        private bool _isFullScreen;
        private double _seekBarValue;
        private string _videoDuration;
        private string _fileName;
        private string _statusMessage;
        private string _searchText;
        private string _playingVideo;
        private VideoFile _selectedVideo;
        private MediaElement _mediaElement;
        private ObservableCollection<FolderViewModel> _filteredFolders;
        private HttpClient _httpClient;
        private static readonly List<string> RootPaths = ["C:\\Users", "D:\\", "G:\\"];
        private static readonly HashSet<string> VideoExtensions = [".mp4", ".avi", ".mkv"];
        private readonly string artifactoryUrl = "https://backupvideos.jfrog.io/artifactory/BackupVideos/";
        private readonly string apiKey = "AKCpBtMKQFzBQtm6r43Q7PMRqLUqFpz3vdsVtVsSXqGiYTUdMRNhAYf5SGSYxeBVyJknQUZFQ"; // Replace with your actual API key
        #endregion

    }
}
