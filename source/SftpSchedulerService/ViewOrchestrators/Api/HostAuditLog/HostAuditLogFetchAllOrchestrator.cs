using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Models.JobFileLog;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.ViewOrchestrators.Api.HostAuditLog
{
    public interface IHostAuditLogFetchAllOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(string hostHash);
    }

    public class HostAuditLogFetchAllOrchestrator : IHostAuditLogFetchAllOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IHostAuditLogRepository _hostAuditLogRepo;

        public const int DefaultRowCount = 50;

        public HostAuditLogFetchAllOrchestrator(IDbContextFactory dbContextFactory, IHostAuditLogRepository hostAuditLogRepo)
        {
            _dbContextFactory = dbContextFactory;
            _hostAuditLogRepo = hostAuditLogRepo;
        }

        public async Task<IActionResult> Execute(string hostHash)
        {
            int hostId = UrlUtils.Decode(hostHash);

            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var auditLogs = await _hostAuditLogRepo.GetByAllHostAsync(dbContext, hostId);
                var result = HostAuditLogMapper.MapToViewModelCollection(auditLogs);
                return new OkObjectResult(result);
            }
        }
    }
}
