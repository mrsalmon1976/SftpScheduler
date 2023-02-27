using System;
using System.Collections.Generic;
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

        public virtual string? Name { get; set; }

        public virtual int? HostId { get; set; }

        public virtual DateTime Created { get; internal set; }
    }
}
