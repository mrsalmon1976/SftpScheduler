using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.Common.IO
{
    public interface IFileUtility
    {
        void Copy(string sourceFileName, string destFileName);

        void Delete(string path);

        bool Exists(string? path);

        void ExtractZipFile(string sourceArchiveFileName, string destinationDirectoryName);

        void Move(string source, string target, bool overwrite);

        string ReadAllText(string path);

        void WriteAllText(string path, string contents);
    }

    public class FileUtility : IFileUtility
    {

        public void Copy(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }


        public virtual bool Exists(string? path)
        {
            return File.Exists(path);
        }

        public void ExtractZipFile(string sourceArchiveFileName, string destinationDirectoryName)
        {
            ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName);
        }

        public void Move(string source, string target, bool overwrite)
        {
            if (overwrite && File.Exists(target))
            {
                File.Delete(target);
            }
            File.Move(source, target);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }
    }
}
