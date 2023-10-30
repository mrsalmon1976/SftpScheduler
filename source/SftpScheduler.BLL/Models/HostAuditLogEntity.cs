using System.ComponentModel.DataAnnotations;

namespace SftpScheduler.BLL.Models
{
    public class HostAuditLogEntity
    {
        public HostAuditLogEntity()
        {
            this.Id = 0;
            this.HostId = 0;
            this.PropertyName = String.Empty;
            this.FromValue = String.Empty;
            this.ToValue = String.Empty;
            this.UserName = String.Empty;
            this.Created = DateTime.UtcNow;
        }

        public HostAuditLogEntity(int hostId, string propertyName, string? fromValue, string? toValue, string? userName) : this()
        {
            this.HostId = hostId;
            this.PropertyName = propertyName;
            this.FromValue = fromValue ?? String.Empty;
            this.ToValue = toValue ?? String.Empty;
            this.UserName = userName ?? String.Empty;
        }

        public virtual int Id { get; set; }

        [Required]
        public virtual int HostId { get; set; }

        [Required]
        public virtual string PropertyName { get; set; }

        [Required]
        public virtual string FromValue { get; set; }

        [Required]
        public virtual string ToValue { get; set; }

        [Required]
        public virtual string UserName { get; set; }

        public virtual DateTime Created { get; set; }
    }
}
