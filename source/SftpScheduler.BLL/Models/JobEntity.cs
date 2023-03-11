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

        [Required]
        [CronSchedule]
        public virtual string Schedule { get; set; }

        public virtual DateTime Created { get; internal set; }
    }
}
