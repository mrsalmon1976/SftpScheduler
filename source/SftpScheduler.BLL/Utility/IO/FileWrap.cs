using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Utility.IO
{
    public interface IFileWrap
    {
        void Move(string sourceFileName, string destFileName);

    }

    public class FileWrap : IFileWrap
    {
        public void Move(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }
    }
}
