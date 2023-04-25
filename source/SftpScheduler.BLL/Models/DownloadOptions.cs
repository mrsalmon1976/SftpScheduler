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
        public DownloadOptions(int jobId, string localPath, string remotePath) 
        {
            this.JobId = jobId;
            this.LocalPath = localPath;
            this.RemotePath = remotePath;
            this.LocalCopyPaths = new List<string>();
        }    

        public int JobId { get; set; }
        public string LocalPath { get; set; }

        public string RemotePath { get; set; }

        public string? RemoteArchivePath { get; set; }
        
        public bool DeleteAfterDownload { get; set; }

        public List<string> LocalCopyPaths { get; private set; }
    }
}
