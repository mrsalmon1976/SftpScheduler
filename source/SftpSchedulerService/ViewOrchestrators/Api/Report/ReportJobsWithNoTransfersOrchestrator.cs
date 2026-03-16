using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.ViewOrchestrators.Api.Report
{
    public interface IReportJobsWithNoTransfersOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(DateTime startDate, DateTime endDate);
    }

    public class ReportJobsWithNoTransfersOrchestrator : IReportJobsWithNoTransfersOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly JobRepository _jobRepository;

        public ReportJobsWithNoTransfersOrchestrator(IDbContextFactory dbContextFactory, JobRepository jobRepository)
        {
            _dbContextFactory = dbContextFactory;
            _jobRepository = jobRepository;
        }

        public async Task<IActionResult> Execute(DateTime startDate, DateTime endDate)
        {
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var jobEntities = await _jobRepository.GetAllWithoutTransfersBetweenAsync(dbContext, startDate.ToUniversalTime(), endDate.ToUniversalTime());
                var result = jobEntities.Select(x => x.ToViewModel()).ToArray();
                return new OkObjectResult(result);
            }
        }
    }
}
