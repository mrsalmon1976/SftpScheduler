using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.ViewOrchestrators.Api.Job
{
    public interface IJobDeleteOneOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(string hash);
    }

    public class JobDeleteOneOrchestrator : IJobDeleteOneOrchestrator
    {
        private readonly ILogger<JobDeleteOneOrchestrator> _logger;
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IDeleteJobCommand _deleteJobCommand;

        public JobDeleteOneOrchestrator(ILogger<JobDeleteOneOrchestrator> logger, IDbContextFactory dbContextFactory, IDeleteJobCommand deleteJobCommand)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _deleteJobCommand = deleteJobCommand;
        }

        public async Task<IActionResult> Execute(string hash)
        {
            int jobId = UrlUtils.Decode(hash);
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                await _deleteJobCommand.ExecuteAsync(dbContext, jobId);
                _logger.LogInformation("Deleted job {jobId}", jobId);
                return new OkResult();
            }
        }
    }
}
