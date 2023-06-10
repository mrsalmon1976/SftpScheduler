using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Models.Dashboard;

namespace SftpSchedulerService.ViewOrchestrators.Api.Dashboard
{
    public interface IDashboardFetchAllOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute();
    }

    public class DashboardFetchAllOrchestrator : IDashboardFetchAllOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly JobRepository _jobRepository;

        public DashboardFetchAllOrchestrator(IDbContextFactory dbContextFactory, JobRepository jobRepository)
        {
            _dbContextFactory = dbContextFactory;
            _jobRepository = jobRepository;
        }

        public async Task<IActionResult> Execute()
        {
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var totalJobCountTask = _jobRepository.GetAllCountAsync(dbContext);
                var failingJobsTask = _jobRepository.GetAllFailingActiveAsync(dbContext);

                await Task.WhenAll(totalJobCountTask, failingJobsTask);

                DashboardViewModel result = new DashboardViewModel();
                result.JobTotalCount = totalJobCountTask.Result;
                result.JobFailingCount = failingJobsTask.Result.Count();

                return new OkObjectResult(result);
            }
        }
    }
}
