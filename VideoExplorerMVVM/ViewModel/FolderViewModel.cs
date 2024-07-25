using System.Collections.ObjectModel;
using System.IO;
using VideoExplorerMVVM.Model;

namespace VideoExplorerMVVM.ViewModel
{
    public class FolderViewModel : ViewModelBase
    {
        public FolderViewModel(string folderPath)
        {
            FolderPath = folderPath;
            Videos = new ObservableCollection<VideoFile>();
        }

        public string FolderPath { get; }
        public string DisplayFolderName => GetDisplayFolderName();
        public ObservableCollection<VideoFile> Videos { get; }

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
    }
}
