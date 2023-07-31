using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.Models.JobFileLog
{
    public class JobFileLogMapper
    {
        public static JobFileLogViewModel MapToViewModel(JobFileLogEntity logEntity)
        {
            JobFileLogViewModel viewModel = new JobFileLogViewModel();
            viewModel.Id = logEntity.Id;
            viewModel.JobId = logEntity.JobId;
            viewModel.FileName = logEntity.FileName;
            viewModel.FileLength = logEntity.FileLength;
            viewModel.StartDate = logEntity.StartDate;
            viewModel.EndDate = logEntity.EndDate;
            return viewModel;
        }

        public static IEnumerable<JobFileLogViewModel> MapToViewModelCollection(IEnumerable<JobFileLogEntity> logEntities)
        {
            return logEntities.Select(x => MapToViewModel(x));
        }
    }
}
