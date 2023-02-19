using SftpScheduler.BLL.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Models
{
    public class HostEntity
    {
        public HostEntity()
        {
            this.Id = 0;
            this.Created = DateTime.UtcNow;
        }

        public virtual int Id { get; internal set; }

        [Required]
        public virtual string? Name { get; set; }

        [Required]
        [Host]
        public virtual string? Host { get; set; }

        [Required]
        [Range(1, 65535)]
        public virtual int? Port { get; set; }

        public virtual string? Username { get; set; }

        public virtual string? Password { get; set; }

        public virtual DateTime Created { get; internal set; }
    }
}
