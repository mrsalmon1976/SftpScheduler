using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.Models.JobFileLog;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.ViewOrchestrators.Api.JobFileLog
{
    public interface IJobFileLogFetchAllOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(string jobHash, int? maxLogId);
    }

    public class JobFileLogFetchAllOrchestrator : IJobFileLogFetchAllOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly JobFileLogRepository _jobFileLogRepo;

        public const int DefaultRowCount = 50;

        public JobFileLogFetchAllOrchestrator(IDbContextFactory dbContextFactory, JobFileLogRepository jobFileLogRepo)
        {
            _dbContextFactory = dbContextFactory;
            _jobFileLogRepo = jobFileLogRepo;
        }

        public async Task<IActionResult> Execute(string jobHash, int? maxLogId)
        {
            int jobId = UrlUtils.Decode(jobHash);

            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var jobEntities = await _jobFileLogRepo.GetAllByJobAsync(dbContext, jobId, maxLogId ?? Int32.MaxValue, DefaultRowCount);
                var result = JobFileLogMapper.MapToViewModelCollection(jobEntities);
                return new OkObjectResult(result);
            }
        }
    }
}
