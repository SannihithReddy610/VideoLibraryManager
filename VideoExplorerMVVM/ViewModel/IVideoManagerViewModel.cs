using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibraryManager.ViewModel
{
    public interface IVideoManagerViewModel
    {
        Task LoadVideosAsync();
        Task LoadCloudVideosAsync();
        Task SyncVideosAsync();
        Task FilterVideosAsync();
    }
}
