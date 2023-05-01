using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Caching;
using SftpSchedulerService.Models.Notification;

namespace SftpSchedulerService.ViewOrchestrators.Api.Job
{
    public interface IJobNotificationFetchAllOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(bool forceReload);
    }

    public class JobNotificationFetchAllOrchestrator : IJobNotificationFetchAllOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ICacheProvider _cacheProvider;
        private readonly JobRepository _jobRepo;

        public JobNotificationFetchAllOrchestrator(IDbContextFactory dbContextFactory, ICacheProvider cacheProvider, JobRepository jobRepo)
        {
            _dbContextFactory = dbContextFactory;
            _cacheProvider = cacheProvider;
            _jobRepo = jobRepo;
        }

        public async Task<IActionResult> Execute(bool forceReload)
        {
            List<JobNotificationViewModel>? notifications = _cacheProvider.Get<List<JobNotificationViewModel>>(CacheKeys.JobNotifications);

            if (notifications == null || forceReload) {

                using (IDbContext dbContext = _dbContextFactory.GetDbContext())
                {
                    var failingJobsTask = _jobRepo.GetAllFailingAsync(dbContext);
                    var warningJobsTask = _jobRepo.GetAllFailedSinceAsync(dbContext, DateTime.Now.AddDays(-1));

                    await Task.WhenAll(failingJobsTask, warningJobsTask);

                    notifications = new List<JobNotificationViewModel>();

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

                    _cacheProvider.Set(CacheKeys.JobNotifications, notifications, TimeSpan.FromMinutes(5));
                }
            }

            return new OkObjectResult(notifications);
        }
    }
}
