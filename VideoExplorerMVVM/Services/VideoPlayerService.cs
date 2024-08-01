using System.Windows.Controls;

namespace VideoLibraryManager.Services
{
    public class VideoPlayerService : IVideoPlayerService
    {
        /// <summary>
        /// Plays the selected video. If the video is paused, it resumes playback.
        /// Otherwise, it starts playing from the beginning.
        /// </summary>
        public void PlayVideo(bool isPaused, MediaElement mediaElement, string filePath)
        {
            if (isPaused)
            {
                mediaElement.Play();
            }
            else
            {
                mediaElement.Source = new Uri(filePath);
                mediaElement.Play();
            }
        }

        /// <summary>
        /// Pauses the currently playing video.
        /// </summary>

        public void Pause(MediaElement mediaElement)
        {
            mediaElement.Pause();
        }

        /// <summary>
        /// Stops the currently playing video.
        /// </summary>
        public void Stop(MediaElement mediaElement)
        {
            mediaElement.Stop();
        }
    }
}
