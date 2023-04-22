using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.ViewOrchestrators.Api.Host
{
    public class HostDeleteOneOrchestrator
    {
        private readonly ILogger<HostDeleteOneOrchestrator> _logger;
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IDeleteHostCommand _deleteHostCommand;

        public HostDeleteOneOrchestrator(ILogger<HostDeleteOneOrchestrator> logger, IDbContextFactory dbContextFactory, IDeleteHostCommand deleteHostCmd)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _deleteHostCommand = deleteHostCmd;
        }

        public async Task<IActionResult> Execute(string hash)
        {
            int hostId = UrlUtils.Decode(hash);
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var hostEntity = await _deleteHostCommand.ExecuteAsync(dbContext, hostId);
                _logger.LogInformation($"Deleted job {hostId}");
                return new OkResult();
            }
        }
    }
}
