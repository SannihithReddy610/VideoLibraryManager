using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using VideoLibraryManager.Model;
using VideoLibraryManager.ViewModel;
using static System.Windows.MessageBox;

namespace VideoLibraryManager.Services
{
    public class CloudVideoFileManagementService : ICloudVideoFileManagementService
    {

        public CloudVideoFileManagementService()
        {
            _artifactoryKey = Environment.GetEnvironmentVariable("JFROG_API_KEY");
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-JFrog-Art-Api", _artifactoryKey);
            _loadInputConfiguration = LoadConfiguration();
        }
        /// <summary>
        /// Downloads a video from the cloud to the user's local documents folder.
        /// </summary>
        public async Task DownloadVideo(string cloudSelectedFileName)
        {
            var artifactoryUrl = _loadInputConfiguration["ArtifactoryUrl"];
            string downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), cloudSelectedFileName);
            using (var response = await _httpClient.GetAsync($"{artifactoryUrl}{cloudSelectedFileName}",
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
        /// Deletes a video file from the cloud and its previous version, if it exists.
        /// </summary>
        public async Task DeleteCloudFileAsync(string cloudSelectedFileName)
        {
            var artifactoryUrl = _loadInputConfiguration["ArtifactoryUrl"];
            var artifactoryVersionUrl = _loadInputConfiguration["artifactoryVersionUrl"];
            var response = await _httpClient.DeleteAsync($"{artifactoryUrl}{cloudSelectedFileName}");
            response.EnsureSuccessStatusCode();
            try
            {
                await _httpClient.DeleteAsync($"{artifactoryVersionUrl}{cloudSelectedFileName}");
            }
            catch
            {

            }
        }

        /// <summary>
        /// Uploads a new version of a video to the cloud.
        /// </summary>
        public async Task UploadNewVersionOfVideo(string cloudSelectedFileName)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.avi;*.mov;*.wmv;*.flv;*.mkv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                await MoveVideoFile(cloudSelectedFileName);
                await UploadVideo(filePath, cloudSelectedFileName);
            }
        }

        /// <summary>
        /// Downloads a previous version of a video from the cloud to the user's local documents folder.
        /// </summary>
        public async Task DownloadPreviousVersionOfVideo(string cloudSelectedFileName)
        {
            //var fileName = videoManagerViewModel.CloudSelectedVideo.FileName;
            //var configuration = videoManagerViewModel.LoadInputConfiguration;
            var artifactoryVersionUrl = _loadInputConfiguration["artifactoryVersionUrl"];
            var downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), cloudSelectedFileName);
            using (var response = await _httpClient.GetAsync($"{artifactoryVersionUrl}{cloudSelectedFileName}",
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

        public async Task<string> LoadCloudVideosAsync()
        {
            return await _httpClient.GetStringAsync(_loadInputConfiguration["ArtifactoryUrl"]);
        }

        /// <summary>
        /// Move Video from one folder to other on cloud
        /// </summary>
        /// <returns></returns>
        private async Task MoveVideoFile(string fileName)
        {
            try
            {
                //var fileName = videoManagerViewModel.CloudSelectedVideo.FileName;
                //var configuration = videoManagerViewModel.LoadInputConfiguration;
                var artifactoryUrl = _loadInputConfiguration["ArtifactoryUrl"];
                var artifactoryVersionUrl = _loadInputConfiguration["artifactoryVersionUrl"];
                var response = await _httpClient.GetAsync($"{artifactoryUrl}{fileName}",
                    HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    var fileContent = await response.Content.ReadAsStreamAsync();

                    using (var content = new StreamContent(fileContent))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        _ = await _httpClient.PutAsync(artifactoryVersionUrl + fileName, content);
                    }
                    _ = _httpClient.DeleteAsync($"{artifactoryUrl}{fileName}");

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
        private async Task UploadVideo(string filePath, string cloudSelectedFileName)
        {
            try
            {
                //var configuration = videoManagerViewModel.LoadInputConfiguration;
                var artifactoryUrl = _loadInputConfiguration["ArtifactoryUrl"];
                using (var content = new StreamContent(File.OpenRead(filePath)))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    await _httpClient.PutAsync(artifactoryUrl + cloudSelectedFileName, content);
                }
                Show("New version uploaded successfully");
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }

        }

        /// <summary>
        /// Uploads the selected video file to the cloud.
        /// </summary>
        public async Task<HttpResponseMessage> UploadVideo(string filePath)
        {
            //var filePath = videoManagerViewModel.SelectedVideo.FilePath;
            var fileName = Path.GetFileName(filePath);

            var artifactoryUrl = _loadInputConfiguration["ArtifactoryUrl"];

            using (var content = new StreamContent(File.OpenRead(filePath)))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response = await _httpClient.PutAsync(artifactoryUrl + fileName, content);

                return response;
            }
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

        private IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

        private HttpClient _httpClient;
        private string _artifactoryKey;
        private IConfiguration _loadInputConfiguration;
        private readonly string? _artifactoryUrl;
    }
}
