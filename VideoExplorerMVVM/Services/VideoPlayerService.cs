using VideoLibraryManager.Model;
using VideoLibraryManager.ViewModel;

namespace VideoLibraryManager.Services
{
    public class VideoPlayerService(VideoManagerViewModel videoManagerViewModel) : IVideoPlayerService
    {
        /// <summary>
        /// Plays the selected video. If the video is paused, it resumes playback.
        /// Otherwise, it starts playing from the beginning.
        /// </summary>
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

        /// <summary>
        /// Plays the specified video when it is double-clicked.
        /// </summary>
        /// <param name="videoFile">The video file to play.</param>
        public void PlayVideoOnDoubleClick(VideoFile videoFile)
        {
            videoManagerViewModel.SelectedVideo = videoFile;
            PlayVideo();
        }

        /// <summary>
        /// Pauses the currently playing video.
        /// </summary>

        public void Pause()
        {
            videoManagerViewModel.MediaElement.Pause();
            videoManagerViewModel.IsPaused = true;
        }

        /// <summary>
        /// Stops the currently playing video.
        /// </summary>
        public void Stop()
        {
            videoManagerViewModel.MediaElement.Stop();
        }

        /// <summary>
        /// Toggles the full-screen mode for the video player.
        /// </summary>
        public void ToggleFullScreen()
        {
            videoManagerViewModel.IsFullScreen = !videoManagerViewModel.IsFullScreen;
        }
    }
}
