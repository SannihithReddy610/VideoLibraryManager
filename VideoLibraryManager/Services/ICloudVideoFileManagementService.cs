﻿using System.Net.Http;

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
