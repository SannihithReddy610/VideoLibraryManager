using System.Windows.Controls;

namespace VideoLibraryManager.Services
{
    public  interface IVideoPlayerService
    {
        void PlayVideo(bool isPaused, MediaElement mediaElement, string filePath);
        void Pause(MediaElement mediaElement);
        void Stop(MediaElement mediaElement);
    }
}
