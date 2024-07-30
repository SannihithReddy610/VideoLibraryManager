#region Directives
using System.ComponentModel;
#endregion

namespace VideoLibraryManager.Model
{
    public class CloudVideoFile : ICloudVideoFile
    {
        #region Constructor
        public CloudVideoFile(string fileName)
        {
            FileName = fileName;
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
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Private Fields
        private string _fileName;
        #endregion
    }
}