using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Models
{
    public class JobResultEntity
    {
        public int Id { get; set; }

        public int JobId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int Progress { get; set; }

        public string? Status { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
