using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibraryManager.Services
{
    public interface ICloudVideoFileManagementService
    {
        Task DownloadVideo();
        Task DeleteCloudFileAsync();
        Task UploadNewVersionOfVideo();
        Task DownloadPreviousVersionOfVideo();
    }
}
