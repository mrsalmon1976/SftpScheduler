using SftpScheduler.BLL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Models
{
    public class UploadOptions
    {
        public UploadOptions() 
        {
			this.LocalFilePaths = new List<string>();
            this.RemotePath = String.Empty;
            this.PreserveTimestamp = true;
        }    

        public int JobId { get; set; }

		public List<string> LocalFilePaths { get; private set; }

		public string RemotePath { get; set; }

        public string? LocalArchivePath {  get; set; }

        public string? LocalPrefix { get; set; }

        public bool RestartOnFailure { get; set; }

        public string? FileMask { get; set; }

        public bool PreserveTimestamp { get; set; } 

        public TransferMode TransferMode { get; set; }

        public CompressionMode CompressionMode { get; set; }    

    }
}
