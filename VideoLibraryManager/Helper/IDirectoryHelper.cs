using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibraryManager.Helper
{
    public interface IDirectoryHelper
    {
        bool DoesDirectoryExist(string path);
        bool DoesFileExist(string path);
        string[] GetFiles(string path);
        string[] GetDirectories(string path);
        void MoveFile(string source, string destination);
        void DeleteFile(string path);

    }
}
