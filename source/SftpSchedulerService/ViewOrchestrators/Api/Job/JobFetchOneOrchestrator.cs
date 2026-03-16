using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.ViewOrchestrators.Api.Job
{
    public interface IJobFetchOneOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(string hash);
    }

    public class JobFetchOneOrchestrator : IJobFetchOneOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly JobRepository _jobRepository;

        public JobFetchOneOrchestrator(IDbContextFactory dbContextFactory, JobRepository jobRepository)
        {
            _dbContextFactory = dbContextFactory;
            _jobRepository = jobRepository;
        }

        public async Task<IActionResult> Execute(string hash)
        {
            int jobId = UrlUtils.Decode(hash);
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var jobEntity = await _jobRepository.GetByIdAsync(dbContext, jobId);
                var result = jobEntity.ToViewModel();
                return new OkObjectResult(result);
            }
        }
    }
}
