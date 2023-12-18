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
        public UploadOptions(int jobId, IEnumerable<string> localFilePaths, string remotePath, bool restartOnFailure) 
        {
            this.JobId = jobId;
			this.LocalFilePaths = new List<string>(localFilePaths);
			this.RemotePath = remotePath;
            this.RestartOnFailure = restartOnFailure;
        }    

        public int JobId { get; set; }

		public List<string> LocalFilePaths { get; private set; }

		public string RemotePath { get; set; }

        public bool RestartOnFailure { get; set; }

    }
}
