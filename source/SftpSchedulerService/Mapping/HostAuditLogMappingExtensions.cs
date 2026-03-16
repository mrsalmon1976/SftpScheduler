using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.HostAuditLog;

namespace SftpSchedulerService.Mapping
{
    public static class HostAuditLogMappingExtensions
    {
        public static HostAuditLogViewModel ToViewModel(this HostAuditLogEntity hostAuditLogEntity)
        {
            return new HostAuditLogViewModel
            {
                Id = hostAuditLogEntity.Id,
                HostId = hostAuditLogEntity.HostId,
                PropertyName = hostAuditLogEntity.PropertyName,
                FromValue = hostAuditLogEntity.FromValue,
                ToValue = hostAuditLogEntity.ToValue,
                UserName = hostAuditLogEntity.UserName,
                Created = hostAuditLogEntity.Created,
            };
        }
    }
}
