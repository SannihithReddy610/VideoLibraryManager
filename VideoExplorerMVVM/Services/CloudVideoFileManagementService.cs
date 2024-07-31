using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Win32;
using VideoLibraryManager.ViewModel;
using static System.Windows.MessageBox;

namespace VideoLibraryManager.Services
{
    public class CloudVideoFileManagementService(VideoManagerViewModel videoManagerViewModel) : ICloudVideoFileManagementService
    {
        public async Task DownloadVideo()
        {
            var fileName = videoManagerViewModel.CloudSelectedVideo.FileName;
            var configuration = videoManagerViewModel.LoadInputConfiguration;
            var artifactoryUrl = configuration["ArtifactoryUrl"];
            string downloadPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            using (var response = await videoManagerViewModel.HttpClient.GetAsync($"{artifactoryUrl}{fileName}",
                       HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var fileStream =
                       new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }
        }

        public async Task DeleteCloudFileAsync()
        {
            var fileName = videoManagerViewModel.CloudSelectedVideo.FileName;
            var configuration = videoManagerViewModel.LoadInputConfiguration;
            var artifactoryUrl = configuration["ArtifactoryUrl"];
            var artifactoryVersionUrl = configuration["artifactoryVersionUrl"];
            var response = await videoManagerViewModel.HttpClient.DeleteAsync($"{artifactoryUrl}{fileName}");
            response.EnsureSuccessStatusCode();
            //Delete previous version of deleted file
            try
            {
                await videoManagerViewModel.HttpClient.DeleteAsync($"{artifactoryVersionUrl}{fileName}");
            }
            catch
            {

            }
        }

        public async Task UploadNewVersionOfVideo()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.avi;*.mov;*.wmv;*.flv;*.mkv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                await MoveVideoFile();
                await UploadVideo(filePath);
            }
        }

        public async Task DownloadPreviousVersionOfVideo()
        {
            var fileName = videoManagerViewModel.CloudSelectedVideo.FileName;
            var configuration = videoManagerViewModel.LoadInputConfiguration;
            var artifactoryVersionUrl = configuration["artifactoryVersionUrl"];
            var downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            using (var response = await videoManagerViewModel.HttpClient.GetAsync($"{artifactoryVersionUrl}{fileName}",
                       HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var fileStream =
                       new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }
        }

        /// <summary>
        /// Move Video from one folder to other on cloud
        /// </summary>
        /// <returns></returns>
        private async Task MoveVideoFile()
        {
            try
            {
                var fileName = videoManagerViewModel.CloudSelectedVideo.FileName;
                var configuration = videoManagerViewModel.LoadInputConfiguration;
                var artifactoryUrl = configuration["ArtifactoryUrl"];
                var artifactoryVersionUrl = configuration["artifactoryVersionUrl"];
                var response = await videoManagerViewModel.HttpClient.GetAsync($"{artifactoryUrl}{fileName}",
                    HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    var fileContent = await response.Content.ReadAsStreamAsync();

                    using (var content = new StreamContent(fileContent))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        _ = await videoManagerViewModel.HttpClient.PutAsync(artifactoryVersionUrl + fileName, content);
                    }
                    _ = videoManagerViewModel.HttpClient.DeleteAsync($"{artifactoryUrl}{fileName}");

                }
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }

        /// <summary>
        /// Upload video to cloud
        /// </summary>
        /// <param name="filePath"></param>
        private async Task UploadVideo(string filePath)
        {
            try
            {
                var configuration = videoManagerViewModel.LoadInputConfiguration;
                var artifactoryUrl = configuration["ArtifactoryUrl"];
                using (var content = new StreamContent(File.OpenRead(filePath)))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    await videoManagerViewModel.HttpClient.PutAsync(artifactoryUrl + videoManagerViewModel.CloudSelectedVideo.FileName, content);
                }
                Show("New version uploaded successfully");
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }

        }
    }
}
