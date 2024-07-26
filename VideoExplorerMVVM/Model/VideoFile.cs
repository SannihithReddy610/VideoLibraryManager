#region Directives
using NAudio.Wave;
using System.IO;
#endregion

namespace VideoExplorerMVVM.Model
{
    public class VideoFile
    {
        #region Constructor
        public VideoFile(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            FileType = Path.GetExtension(filePath);
            FileSize = (long)GetFileSizeInMB(new FileInfo(filePath).Length);
            DateModified = File.GetLastWriteTime(filePath);
            Duration = GetVideoDuration(filePath);
        }
        #endregion

        #region Properties
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FolderPath { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime DateModified { get; set; }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets the video file duration
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private TimeSpan GetVideoDuration(string filePath)
        {
            try
            {
                using (var reader = new MediaFoundationReader(filePath))
                {
                    return reader.TotalTime;
                }
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Returns the file size in MB
        /// </summary>
        /// <param name="fileSizeInBytes"></param>
        private double GetFileSizeInMB(long fileSizeInBytes)
        {
            // Convert to MB and round to 2 decimal places
            return Math.Round(fileSizeInBytes / (1024.0 * 1024.0), 2); 
        }
        #endregion
    }
}
