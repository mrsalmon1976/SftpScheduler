using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Models
{
    public class JobEntity
    {
        public JobEntity()
        {
            this.Id = 0;
            this.Created = DateTime.UtcNow;
        }

        public virtual int Id { get; internal set; }

        [Required]
        public virtual string? Name { get; set; }

        [Required]
        [Range(1, Int32.MaxValue)]
        public virtual int? HostId { get; set; }

        [Required(ErrorMessage = "Job type is required")]
        [Range(1, 2, ErrorMessage = "Job type must be 1 (Download) or 2 (Upload)")]
        public virtual JobType Type { get; set; }

        [Required]
        [CronSchedule]
        public virtual string Schedule { get; set; }

        // not marked as required as this is a "cached" value and system-handled
        public virtual string ScheduleInWords { get; internal set; }

        [Required(ErrorMessage = "Local path is required")]  
        public virtual string LocalPath { get; set; }

        [Required(ErrorMessage = "Remote path is required")]
        public virtual string RemotePath { get; set; }

        public virtual DateTime Created { get; internal set; }
    }
}
