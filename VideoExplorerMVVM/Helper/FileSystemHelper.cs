using System.IO;

namespace VideoLibraryManager.Helper
{
    public class FileSystemHelper : IDirectoryHelper
    {
        public bool DoesDirectoryExist(string path)
        {
            return Directory.Exists(path);
        }

        public bool DoesFileExist(string path)
        {
            return File.Exists(path);
        }

        public string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public void MoveFile(string source, string destination)
        {
            File.Move(source, destination);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }
    }
}
