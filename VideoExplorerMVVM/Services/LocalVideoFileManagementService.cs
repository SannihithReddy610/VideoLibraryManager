using System.Collections.Concurrent;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VideoLibraryManager.Helper;
using VideoLibraryManager.Model;
using static System.Windows.MessageBox;

namespace VideoLibraryManager.Services
{
    public class LocalVideoFileManagementService : ILocalVideoFileManagementService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _loadInputConfiguration;
        private readonly List<string>? _rootPaths;
        private readonly HashSet<string> _videoExtensions;
        private readonly IDirectoryHelper _directoryHelper;

        public LocalVideoFileManagementService(ILogger logger) : this(logger, new FileSystemHelper())
        {
            
        }

        public LocalVideoFileManagementService(ILogger logger, IDirectoryHelper directoryHelper)
        {
            _logger = logger;
            _loadInputConfiguration = LoadConfiguration();
            _rootPaths = _loadInputConfiguration.GetSection("RootPaths").Get<List<string>>();
            _videoExtensions = new HashSet<string>(_loadInputConfiguration.GetSection("VideoExtensions").Get<List<string>>());
            _directoryHelper = directoryHelper;
        }

        public async Task<IEnumerable<VideoFile>> LoadVideosAsync()
        {
            return await GetVideoFilesAsync(_rootPaths).ConfigureAwait(false);
        }

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
                if (_directoryHelper.DoesFileExist(rootPath))
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
                var fileEntries = _directoryHelper.GetFiles(path);
                lock (files)
                {
                    files.AddRange(fileEntries);
                }

                var directoryEntries = _directoryHelper.GetDirectories(path);
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
        /// Renames the selected video file.
        /// </summary>
        public string RenameVideo(string fileName, string filePath, string newFileName)
        {
            if (string.IsNullOrWhiteSpace(newFileName) || newFileName == fileName)
                return filePath;

            string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), newFileName);

            // Check if the new file name already exists
            if (File.Exists(newFilePath))
            {
                Show("A file with the new name already exists. Please choose a different name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return "";
            }

            // Rename the file
            File.Move(filePath, newFilePath);

            return newFilePath;

        }

        /// <summary>
        /// Deletes the selected video file.
        /// </summary>
        public void DeleteVideo(string filePath)
        {

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

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
    }
}
