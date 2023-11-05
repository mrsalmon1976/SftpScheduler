using System.ComponentModel.DataAnnotations;

namespace SftpScheduler.BLL.Models
{
    public class JobAuditLogEntity
    {
        public JobAuditLogEntity()
        {
            this.Id = 0;
            this.JobId = 0;
            this.PropertyName = String.Empty;
            this.FromValue = String.Empty;
            this.ToValue = String.Empty;
            this.UserName = String.Empty;
            this.Created = DateTime.UtcNow;
        }

        public JobAuditLogEntity(int jobId, string propertyName, string? fromValue, string? toValue, string? userName) : this()
        {
            this.JobId = jobId;
            this.PropertyName = propertyName;
            this.FromValue = fromValue ?? String.Empty;
            this.ToValue = toValue ?? String.Empty;
            this.UserName = userName ?? String.Empty;
        }

        public virtual int Id { get; set; }

        [Required]
        public virtual int JobId { get; set; }

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
