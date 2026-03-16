using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.Mapping
{
    public static class JobFileLogMappingExtensions
    {
        public static JobFileLogViewModel ToViewModel(this JobFileLogEntity jobFileLogEntity)
        {
            return new JobFileLogViewModel
            {
                Id = jobFileLogEntity.Id,
                JobId = jobFileLogEntity.JobId,
                FileName = jobFileLogEntity.FileName,
                FileLength = jobFileLogEntity.FileLength,
                StartDate = jobFileLogEntity.StartDate,
                EndDate = jobFileLogEntity.EndDate,
            };
        }
    }
}
