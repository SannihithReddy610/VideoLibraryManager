using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using VideoLibraryManager.Model;

namespace VideoLibraryManager.Services
{
    public  interface IVideoPlayerService
    {
        void PlayVideo(bool isPaused, MediaElement mediaElement, string filePath);
        void Pause(MediaElement mediaElement);
        void Stop(MediaElement mediaElement);
    }
}
