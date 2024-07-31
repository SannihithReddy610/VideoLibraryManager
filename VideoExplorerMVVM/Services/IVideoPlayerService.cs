using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibraryManager.Model;

namespace VideoLibraryManager.Services
{
    public  interface IVideoPlayerService
    {
        void PlayVideo();
        void PlayVideoOnDoubleClick(VideoFile videoFile);
        void Pause();
        void Stop();
        void ToggleFullScreen();

    }
}
