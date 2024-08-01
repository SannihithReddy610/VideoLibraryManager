using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibraryManager.Services
{
    public interface ICloudVideoFileManagementService
    {
        Task DownloadVideo(string cloudSelectedFileName);
        Task DeleteCloudFileAsync(string cloudSelectedFileName);
        Task<HttpResponseMessage> UploadVideo(string filePath);
        Task UploadNewVersionOfVideo(string cloudSelectedFileName);
        Task DownloadPreviousVersionOfVideo(string cloudSelectedFileName);
        Task<string> LoadCloudVideosAsync();
    }
}
