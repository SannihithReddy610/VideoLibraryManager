#region Directives
using NAudio.Wave;
using System.ComponentModel;
using System.IO;
#endregion

namespace VideoLibraryManager.Model
{
    public class VideoFile : INotifyPropertyChanged
    {
        #region Constructor
        public VideoFile(string filePath)
        {
            FilePath = filePath;
            FolderPath = Path.GetDirectoryName(filePath);
            FileName = Path.GetFileName(filePath);
            FileType = Path.GetExtension(filePath);
            FileSize = (long)GetFileSizeInMB(new FileInfo(filePath).Length);
            DateModified = File.GetLastWriteTime(filePath);
            Duration = GetVideoDuration(filePath);
        }
        #endregion

        #region Properties
        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged(nameof(FilePath));
                }
            }
        }

        public string FolderPath
        {
            get => _folderPath;
            set
            {
                if (_folderPath != value)
                {
                    _folderPath = value;
                    OnPropertyChanged(nameof(FolderPath));
                }
            }
        }

        public string FileType
        {
            get => _fileType;
            set
            {
                if (_fileType != value)
                {
                    _fileType = value;
                    OnPropertyChanged(nameof(FileType));
                }
            }
        }

        public long FileSize
        {
            get => _fileSize;
            set
            {
                if (_fileSize != value)
                {
                    _fileSize = value;
                    OnPropertyChanged(nameof(FileSize));
                }
            }
        }

        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    OnPropertyChanged(nameof(Duration));
                }
            }
        }

        public DateTime DateModified
        {
            get => _dateModified;
            set
            {
                if (_dateModified != value)
                {
                    _dateModified = value;
                    OnPropertyChanged(nameof(DateModified));
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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

        #region Private Fields
        private string _fileName;
        private string _folderPath;
        private string _filePath;
        private string _fileType;
        private long _fileSize;
        private TimeSpan _duration;
        private DateTime _dateModified;
        
        #endregion
    }
}
