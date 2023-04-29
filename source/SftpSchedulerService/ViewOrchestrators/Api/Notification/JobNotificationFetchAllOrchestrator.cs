using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Models;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.Models.Notification;

namespace SftpSchedulerService.ViewOrchestrators.Api.Job
{
    public class JobNotificationFetchAllOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly JobRepository _jobRepo;

        public JobNotificationFetchAllOrchestrator(IDbContextFactory dbContextFactory, JobRepository jobRepo)
        {
            _dbContextFactory = dbContextFactory;
            _jobRepo = jobRepo;
        }

        public async Task<IActionResult> Execute()
        {
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var failingJobsTask = _jobRepo.GetAllFailingAsync(dbContext);
                var warningJobsTask = _jobRepo.GetAllFailedSinceAsync(dbContext, DateTime.Now.AddDays(-1));

                await Task.WhenAll(failingJobsTask, warningJobsTask);

                List<JobNotificationViewModel> notifications = new List<JobNotificationViewModel>();

                // run through the errors first
                foreach (JobEntity jobEntity in failingJobsTask.Result) 
                {
                    notifications.Add(new JobNotificationViewModel(jobEntity.Id, jobEntity.Name, AppConstants.NotificationType.Error));
                }

                // add the warnings
                List<int> failingIds = notifications.Select(x => x.JobId).ToList();
                foreach (JobEntity jobEntity in warningJobsTask.Result)
                {
                    if (failingIds.Contains(jobEntity.Id))
                    {
                        continue;
                    }
                    notifications.Add(new JobNotificationViewModel(jobEntity.Id, jobEntity.Name, AppConstants.NotificationType.Warning));
                }

                return new OkObjectResult(notifications);
            }
        }
    }
}
