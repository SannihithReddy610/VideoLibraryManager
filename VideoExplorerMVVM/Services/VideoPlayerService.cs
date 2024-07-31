using VideoLibraryManager.Model;
using VideoLibraryManager.ViewModel;

namespace VideoLibraryManager.Services
{
    public class VideoPlayerService(VideoManagerViewModel videoManagerViewModel) : IVideoPlayerService
    {
        public void PlayVideo()
        {
            if (videoManagerViewModel.IsPaused)
            {
                videoManagerViewModel.MediaElement.Play();
            }
            else
            {
                videoManagerViewModel.MediaElement.Source = new Uri(videoManagerViewModel.SelectedVideo.FilePath);
                videoManagerViewModel.MediaElement.Play();
                videoManagerViewModel.PlayingVideo = videoManagerViewModel.SelectedVideo.FileName;
            }

            videoManagerViewModel.IsPaused = false;
            
        }

        public void PlayVideoOnDoubleClick(VideoFile videoFile)
        {
            videoManagerViewModel.SelectedVideo = videoFile;
            PlayVideo();
        }

        public void Pause()
        {
            videoManagerViewModel.MediaElement.Pause();
            videoManagerViewModel.IsPaused = true;
        }

        public void Stop()
        {
            videoManagerViewModel.MediaElement.Stop();
        }

        public void ToggleFullScreen()
        {
            videoManagerViewModel.IsFullScreen = !videoManagerViewModel.IsFullScreen;
        }
    }
}
