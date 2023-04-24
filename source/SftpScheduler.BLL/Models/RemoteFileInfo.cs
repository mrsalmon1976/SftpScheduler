using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Models
{
    public class RemoteFileInfo
    {
        public RemoteFileInfo()
        {
            this.Name = String.Empty;
            this.FullName = String.Empty;
        }

        public bool IsDirectory { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public long Length { get; set; }

        public DateTime LastWriteTime { get; set; }
    }
}
