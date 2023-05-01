using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
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
        private readonly IMapper _mapper;
        private readonly HostRepository _hostRepository;

        public HostFetchAllOrchestrator(IDbContextFactory dbContextFactory, IMapper mapper, HostRepository hostRepository)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _hostRepository = hostRepository;
        }

        public async Task<IActionResult> Execute()
        {
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var hostEntityTask = _hostRepository.GetAllAsync(dbContext);
                var jobCountTask = _hostRepository.GetAllJobCountsAsync(dbContext);
                await Task.WhenAll(hostEntityTask, jobCountTask);

                var hostEntities = hostEntityTask.Result;
                var jobCounts = jobCountTask.Result;

                // hydrate objects with aggregated values
                var result = _mapper.Map<HostEntity[], HostViewModel[]>(hostEntities.ToArray());
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
