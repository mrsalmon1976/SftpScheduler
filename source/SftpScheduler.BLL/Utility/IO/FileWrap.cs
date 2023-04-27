﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Utility.IO
{
    public interface IFileWrap
    {
        void Copy(string sourceFileName, string destFileName);

        bool Exists(string? path);

        void Move(string sourceFileName, string destFileName);

    }

    public class FileWrap : IFileWrap
    {

        public void Copy(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName);
        }

        public bool Exists(string? path)
        {
            return File.Exists(path);
        }

        public void Move(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }
    }
}
