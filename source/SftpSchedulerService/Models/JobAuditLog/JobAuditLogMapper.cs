using SftpScheduler.BLL.Models;

namespace SftpSchedulerService.Models.JobAuditLog
{
    public class JobAuditLogMapper
    {
        public static JobAuditLogViewModel MapToViewModel(JobAuditLogEntity logEntity)
        {
            JobAuditLogViewModel viewModel = new JobAuditLogViewModel();
            viewModel.Id = logEntity.Id;
            viewModel.JobId = logEntity.JobId;
            viewModel.PropertyName = logEntity.PropertyName;
            viewModel.FromValue = logEntity.FromValue;
            viewModel.ToValue = logEntity.ToValue;
            viewModel.UserName = logEntity.UserName;
            viewModel.Created = logEntity.Created;
            return viewModel;
        }

        public static IEnumerable<JobAuditLogViewModel> MapToViewModelCollection(IEnumerable<JobAuditLogEntity> logEntities)
        {
            return logEntities.Select(x => MapToViewModel(x));
        }
    }
}
