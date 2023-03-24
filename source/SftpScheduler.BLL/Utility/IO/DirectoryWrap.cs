using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Utility.IO
{
    // should be using SystemWrapper library but that targets framework,  not .NET 6
    public interface IDirectoryWrap
    {
        bool Exists(string path);
    }

    public class DirectoryWrap : IDirectoryWrap
    {
        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }
    }
}
