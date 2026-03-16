using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.ViewOrchestrators.Api.Host
{
    public interface IHostFetchOneOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(string hash);
    }

    public class HostFetchOneOrchestrator : IHostFetchOneOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly HostRepository _hostRepository;

        public HostFetchOneOrchestrator(IDbContextFactory dbContextFactory, HostRepository hostRepository)
        {
            _dbContextFactory = dbContextFactory;
            _hostRepository = hostRepository;
        }

        public async Task<IActionResult> Execute(string hash)
        {
            int hostId = UrlUtils.Decode(hash);
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var hostEntity = await _hostRepository.GetByIdAsync(dbContext, hostId);
                var result = hostEntity.ToViewModel();
                return new OkObjectResult(result);
            }
        }
    }
}
