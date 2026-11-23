using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.ViewOrchestrators.Api.Job
{
    public interface IJobFetchAllOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute();
    }

    public class JobFetchAllOrchestrator : IJobFetchAllOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly JobRepository _jobRepository;
        private readonly QuartzTriggerRepository _quartzTriggerRepository;

        public JobFetchAllOrchestrator(IDbContextFactory dbContextFactory, JobRepository jobRepository, QuartzTriggerRepository quartzTriggerRepository)
        {
            _dbContextFactory = dbContextFactory;
            _jobRepository = jobRepository;
            _quartzTriggerRepository = quartzTriggerRepository;
        }

        public async Task<IActionResult> Execute()
        {
            using (IDbContext dbContext = _dbContextFactory.GetDefaultDbContext())
            {
                var jobEntities = await _jobRepository.GetAllAsync(dbContext);
                var failingJobEntities = await _jobRepository.GetAllFailingAsync(dbContext);
                var result = jobEntities.Select(x => x.ToViewModel()).ToArray();

                Dictionary<string, long?> nextFireTimeTicksByTriggerName = await GetNextFireTimeTicksByTriggerNameAsync();

                foreach (JobViewModel jobViewModel in result)
                {
                    if (jobViewModel.IsEnabled
                        && nextFireTimeTicksByTriggerName.TryGetValue(TransferJob.TriggerKeyName(jobViewModel.Id), out long? nextFireTimeTicks)
                        && nextFireTimeTicks.HasValue)
                    {
                        jobViewModel.NextRunTime = new DateTime(nextFireTimeTicks.Value, DateTimeKind.Utc).ToLocalTime();
                    }

                    jobViewModel.IsFailing = failingJobEntities.Any(x => x.Id == jobViewModel.Id);
                }

                return new OkObjectResult(result);
            }
        }

        /// <summary>
        /// Loads the next fire time of every transfer trigger in one query. Calling Quartz's GetTrigger per job
        /// costs one database round-trip per job, which is very slow against the Quartz SQLite store.
        /// </summary>
        private async Task<Dictionary<string, long?>> GetNextFireTimeTicksByTriggerNameAsync()
        {
            using (IDbContext quartzDbContext = _dbContextFactory.GetQuartzDbContext())
            {
                IEnumerable<QuartzTriggerEntity> triggers = await _quartzTriggerRepository.GetAllByGroupAsync(quartzDbContext, QuartzConstants.SchedulerName, TransferJob.GroupName);
                return triggers.ToDictionary(x => x.TriggerName, x => x.NextFireTimeTicks);
            }
        }
    }
}
