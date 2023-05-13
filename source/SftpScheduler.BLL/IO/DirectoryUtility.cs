using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.IO
{
    // should be using SystemWrapper library but that targets framework,  not .NET 6
    public interface IDirectoryUtility
    {

        void Create(string path);

        void Delete(string path, bool recursive);

        IEnumerable<string> EnumerateFiles(string path);

        bool Exists(string path);
    }

    public class DirectoryUtility : IDirectoryUtility
    {

        public void Create(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void Delete(string path, bool recursive)
        {
            Directory.Delete(path, recursive);
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            return Directory.EnumerateFiles(path);
        }

        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

    }
}
