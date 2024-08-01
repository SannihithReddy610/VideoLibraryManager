using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibraryManager.Helper
{
    public interface IDirectoryHelper
    {
        bool DoesFileExist(string path);
        string[] GetFiles(string path);
        string[] GetDirectories(string path);
    }
}
