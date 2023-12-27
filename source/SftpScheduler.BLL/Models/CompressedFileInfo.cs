using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Models
{
    public class CompressedFileInfo
    {
        public CompressionMode CompressionMode { get; set; }

        public string? OriginalFilePath { get; set; }

        public string? CompressedFilePath { get; set; }

    }
}
