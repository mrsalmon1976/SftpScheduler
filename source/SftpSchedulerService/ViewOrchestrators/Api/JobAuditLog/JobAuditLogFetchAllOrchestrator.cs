using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Models.JobAuditLog;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.ViewOrchestrators.Api.JobAuditLog
{
    public interface IJobAuditLogFetchAllOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(string hostHash);
    }

    public class JobAuditLogFetchAllOrchestrator : IJobAuditLogFetchAllOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IJobAuditLogRepository _jobAuditLogRepo;

        public const int DefaultRowCount = 50;

        public JobAuditLogFetchAllOrchestrator(IDbContextFactory dbContextFactory, IJobAuditLogRepository jobAuditLogRepo)
        {
            _dbContextFactory = dbContextFactory;
            _jobAuditLogRepo = jobAuditLogRepo;
        }

        public async Task<IActionResult> Execute(string hostHash)
        {
            int hostId = UrlUtils.Decode(hostHash);

            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var auditLogs = await _jobAuditLogRepo.GetByAllJobAsync(dbContext, hostId);
                var result = JobAuditLogMapper.MapToViewModelCollection(auditLogs);
                return new OkObjectResult(result);
            }
        }
    }
}
