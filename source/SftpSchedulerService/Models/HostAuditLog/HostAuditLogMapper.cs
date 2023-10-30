using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.HostAuditLog;

namespace SftpSchedulerService.Models.JobFileLog
{
    public class HostAuditLogMapper
    {
        public static HostAuditLogViewModel MapToViewModel(HostAuditLogEntity logEntity)
        {
            HostAuditLogViewModel viewModel = new HostAuditLogViewModel();
            viewModel.Id = logEntity.Id;
            viewModel.HostId = logEntity.HostId;
            viewModel.PropertyName = logEntity.PropertyName;
            viewModel.FromValue = logEntity.FromValue;
            viewModel.ToValue = logEntity.ToValue;
            viewModel.UserName = logEntity.UserName;
            viewModel.Created = logEntity.Created;
            return viewModel;
        }

        public static IEnumerable<HostAuditLogViewModel> MapToViewModelCollection(IEnumerable<HostAuditLogEntity> logEntities)
        {
            return logEntities.Select(x => MapToViewModel(x));
        }
    }
}
