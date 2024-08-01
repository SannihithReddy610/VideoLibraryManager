using System.IO;
using System.Windows;
using System.Net.Http;
using System.Net.Http.Headers;
using VideoLibraryManager.ViewModel;
using static System.Windows.MessageBox;

namespace VideoLibraryManager.Services
{
    public class LocalVideoFileManagementService(VideoManagerViewModel videoManagerViewModel)
        : ILocalVideoFileManagementService
    {
        /// <summary>
        /// Renames the selected video file.
        /// </summary>
        public void RenameVideo()
        {
            // Ask the user for a new file name
            string newFileName = Microsoft.VisualBasic.Interaction.InputBox("Enter the new name for the video:", "Rename Video", videoManagerViewModel.SelectedVideo.FileName);

            if (string.IsNullOrWhiteSpace(newFileName) || newFileName == videoManagerViewModel.SelectedVideo.FileName)
                return;

            string newFilePath = Path.Combine(Path.GetDirectoryName(videoManagerViewModel.SelectedVideo.FilePath), newFileName);

            // Check if the new file name already exists
            if (File.Exists(newFilePath))
            {
                Show("A file with the new name already exists. Please choose a different name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Rename the file
            File.Move(videoManagerViewModel.SelectedVideo.FilePath, newFilePath);

            // Update the view model
            videoManagerViewModel.SelectedVideo.FilePath = newFilePath;
            videoManagerViewModel.SelectedVideo.FileName = newFileName;
            
            var folder = videoManagerViewModel.Folders.FirstOrDefault(f => f.Videos.Contains(videoManagerViewModel.SelectedVideo));
            if (folder != null)
            {
                var video = folder.Videos.FirstOrDefault(v => v.FilePath == newFilePath);
                if (video != null)
                {
                    video.FileName = newFileName;
                }
            }
        }

        /// <summary>
        /// Deletes the selected video file.
        /// </summary>
        public void DeleteVideo()
        {
            MessageBoxResult result = Show($"Are you sure you want to permanently delete '{videoManagerViewModel.SelectedVideo.FileName}'?",
                "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                // Delete the video file from the file system
                if (File.Exists(videoManagerViewModel.SelectedVideo.FilePath))
                {
                    File.Delete(videoManagerViewModel.SelectedVideo.FilePath);
                }

                // Remove the video file from the view model
                var folder = videoManagerViewModel.Folders.FirstOrDefault(f => f.Videos.Contains(videoManagerViewModel.SelectedVideo));
                if (folder != null)
                {
                    folder.Videos.Remove(videoManagerViewModel.SelectedVideo);
                    if (!folder.Videos.Any())
                    {
                        videoManagerViewModel.Folders.Remove(folder);
                    }
                }

                // Clear the selected video
                videoManagerViewModel.SelectedVideo = null;
            }
        }

        /// <summary>
        /// Uploads the selected video file to the cloud.
        /// </summary>
        public async Task UploadVideo()
        {
            var filePath = videoManagerViewModel.SelectedVideo.FilePath;
            var fileName = Path.GetFileName(filePath);
            var configuration = videoManagerViewModel.LoadInputConfiguration;
            var artifactoryUrl = configuration["ArtifactoryUrl"];

            using (var content = new StreamContent(File.OpenRead(filePath)))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response = await videoManagerViewModel.HttpClient.PutAsync(artifactoryUrl + fileName, content);

                Show(response.IsSuccessStatusCode
                    ? $"{fileName} uploaded successfully."
                    : $"File upload failed. Status code: {response.StatusCode}");
            }
        }
    }
}
