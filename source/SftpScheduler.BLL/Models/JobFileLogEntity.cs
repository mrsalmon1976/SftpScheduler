using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Models
{
    public class JobFileLogEntity
    {
        public int Id { get; set; }

        public int JobId { get; set; }

        public string FileName { get; set; } = String.Empty;

        public long FileLength { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

    }
}
