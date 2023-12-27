using SftpScheduler.BLL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Models
{
    public class DownloadOptions
    {
        public DownloadOptions() 
        {
            this.LocalPath = String.Empty;
            this.RemotePath = String.Empty;
            this.LocalCopyPaths = new List<string>();
            this.FileMask = String.Empty;  
        }

        public int JobId { get; set; }

        public string LocalPath { get; set; }

        public string RemotePath { get; set; }

        public string? RemoteArchivePath { get; set; }
        
        public bool DeleteAfterDownload { get; set; }

        public List<string> LocalCopyPaths { get; private set; }

        public string FileMask { get; set; }

        public TransferMode TransferMode { get; set; }
    }
}
