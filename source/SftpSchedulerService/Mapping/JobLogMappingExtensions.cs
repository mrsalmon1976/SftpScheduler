using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.Mapping
{
    public static class JobLogMappingExtensions
    {
        public static JobLogViewModel ToViewModel(this JobLogEntity jobLogEntity)
        {
            return new JobLogViewModel
            {
                Id = jobLogEntity.Id,
                JobId = jobLogEntity.JobId,
                StartDate = jobLogEntity.StartDate.ToLocalTime(),
                EndDate = jobLogEntity.EndDate?.ToLocalTime(),
                Progress = jobLogEntity.Progress,
                Status = jobLogEntity.Status,
                ErrorMessage = jobLogEntity.ErrorMessage,
            };
        }
    }
}
