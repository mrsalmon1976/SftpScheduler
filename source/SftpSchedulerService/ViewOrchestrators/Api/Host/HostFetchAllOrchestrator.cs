using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.Host;

namespace SftpSchedulerService.ViewOrchestrators.Api.Host
{
    public interface IHostFetchAllOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute();
    }

    public class HostFetchAllOrchestrator : IHostFetchAllOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly HostRepository _hostRepository;

        public HostFetchAllOrchestrator(IDbContextFactory dbContextFactory, HostRepository hostRepository)
        {
            _dbContextFactory = dbContextFactory;
            _hostRepository = hostRepository;
        }

        public async Task<IActionResult> Execute()
        {
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var hostEntities = await _hostRepository.GetAllAsync(dbContext);
                var jobCounts = await _hostRepository.GetAllJobCountsAsync(dbContext);

                // hydrate objects with aggregated values
                var result = hostEntities.Select(x => x.ToViewModel()).ToArray();
                foreach (HostViewModel hostView in result)
                {
                    HostJobCountEntity? jobCountEntity = jobCounts.SingleOrDefault(x => x.HostId == hostView.Id);
                    if (jobCountEntity != null)
                    {
                        hostView.JobCount = jobCountEntity.JobCount;
                    }
                }

                return new OkObjectResult(result);
            }
        }
    }
}
