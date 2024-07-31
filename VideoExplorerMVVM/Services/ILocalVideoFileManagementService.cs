using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibraryManager.Services
{
    public interface ILocalVideoFileManagementService
    {
        void RenameVideo();
        void DeleteVideo();
        Task UploadVideo();
    }
}
