using System;
using System.ComponentModel;

namespace VideoLibraryManager.Model
{
    public interface ICloudVideoFile : INotifyPropertyChanged
    {
        #region Properties
        string FileName { get; set; }
        #endregion
    }
}