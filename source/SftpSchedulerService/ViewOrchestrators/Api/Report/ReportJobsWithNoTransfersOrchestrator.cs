using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
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
        private readonly IMapper _mapper;
        private readonly JobRepository _jobRepository;

        public ReportJobsWithNoTransfersOrchestrator(IDbContextFactory dbContextFactory, IMapper mapper, JobRepository jobRepository)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _jobRepository = jobRepository;
        }

        public async Task<IActionResult> Execute(DateTime startDate, DateTime endDate)
        {
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var jobEntities = await _jobRepository.GetAllWithoutTransfersBetweenAsync(dbContext, startDate.ToUniversalTime(), endDate.ToUniversalTime());
                var result = _mapper.Map<JobEntity[], JobViewModel[]>(jobEntities.ToArray());
                return new OkObjectResult(result);
            }
        }
    }
}
