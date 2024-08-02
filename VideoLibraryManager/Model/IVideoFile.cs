using System.ComponentModel;

namespace VideoLibraryManager.Model
{
    public interface IVideoFile : INotifyPropertyChanged
    {
        #region Properties
        string FileName { get; set; }
        string FilePath { get; set; }
        string FolderPath { get; set; }
        string FileType { get; set; }
        long FileSize { get; set; }
        TimeSpan Duration { get; set; }
        DateTime DateModified { get; set; }
        #endregion
    }
}