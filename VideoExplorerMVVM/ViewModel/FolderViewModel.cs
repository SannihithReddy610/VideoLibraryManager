#region Directives
using System.Collections.ObjectModel;
using System.IO;
using VideoLibraryManager.Model;

#endregion

namespace VideoLibraryManager.ViewModel
{
    public class FolderViewModel : ViewModelBase
    {
        #region Constructor
        public FolderViewModel(string folderPath)
        {
            FolderPath = folderPath;
            Videos = new ObservableCollection<VideoFile>();
            IsExpanded = true;
        }
        #endregion

        #region Properties
        public string FolderPath { get; }

        public ObservableCollection<VideoFile> Videos { get; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Provides a public getter for the formatted folder name.
        /// </summary>
        public string DisplayFolderName => GetDisplayFolderName();
        #endregion

        #region Private Methods
        /// <summary>
        /// Returns the full path including parent directory or just the folder name if at root.
        /// </summary>
        private string GetDisplayFolderName()
        {
            var directoryInfo = new DirectoryInfo(FolderPath);
            var parentDirectory = directoryInfo.Parent;
            if (parentDirectory != null)
            {
                return $"{parentDirectory.Name}/{directoryInfo.Name}";
            }
            return directoryInfo.Name;
        }
        #endregion

        private bool _isExpanded;
    }
}
