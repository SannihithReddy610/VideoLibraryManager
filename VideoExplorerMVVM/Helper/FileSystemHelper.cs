using System.IO;

namespace VideoLibraryManager.Helper
{
    public class FileSystemHelper : IDirectoryHelper
    {
        public bool DoesFileExist(string path)
        {
            return Directory.Exists(path);
        }

        public string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }
    }
}
