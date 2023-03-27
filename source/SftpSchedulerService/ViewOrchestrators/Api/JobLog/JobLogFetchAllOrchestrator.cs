using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.ViewOrchestrators.Api.JobLog
{
    public class JobLogFetchAllOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IMapper _mapper;
        private readonly JobLogRepository _jobLogRepo;

        public const int DefaultRowCount = 50;

        public JobLogFetchAllOrchestrator(IDbContextFactory dbContextFactory, IMapper mapper, JobLogRepository jobLogRepo)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _jobLogRepo = jobLogRepo;
        }

        public async Task<IActionResult> Execute(string jobHash, int? maxLogId)
        {
            int jobId = UrlUtils.Decode(jobHash);

            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var jobEntities = await _jobLogRepo.GetAllByJobAsync(dbContext, jobId, maxLogId ?? Int32.MaxValue, DefaultRowCount);
                var result = _mapper.Map<JobLogEntity[], JobLogViewModel[]>(jobEntities.ToArray());
                return new OkObjectResult(result);
            }
        }
    }
}
