using System.IO;

namespace VideoLibraryManager.Helper
{
    public class FileSystemHelper : IDirectoryHelper
    {
        public bool DoesFileExist(string path)
        {
            return Directory.Exists(path);
        }
    }
}
