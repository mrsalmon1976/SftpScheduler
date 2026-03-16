using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.JobAuditLog;

namespace SftpSchedulerService.Mapping
{
    public static class JobAuditLogMappingExtensions
    {
        public static JobAuditLogViewModel ToViewModel(this JobAuditLogEntity jobAuditLogEntity)
        {
            return new JobAuditLogViewModel
            {
                Id = jobAuditLogEntity.Id,
                JobId = jobAuditLogEntity.JobId,
                PropertyName = jobAuditLogEntity.PropertyName,
                FromValue = jobAuditLogEntity.FromValue,
                ToValue = jobAuditLogEntity.ToValue,
                UserName = jobAuditLogEntity.UserName,
                Created = jobAuditLogEntity.Created,
            };
        }
    }
}
