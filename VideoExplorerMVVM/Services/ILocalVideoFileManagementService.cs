using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibraryManager.Model;

namespace VideoLibraryManager.Services
{
    public interface ILocalVideoFileManagementService
    {
        Task<IEnumerable<VideoFile>> LoadVideosAsync();
        string RenameVideo(string fileName, string filePath, string newFileName);
        void DeleteVideo(string filePath);
    }
}
