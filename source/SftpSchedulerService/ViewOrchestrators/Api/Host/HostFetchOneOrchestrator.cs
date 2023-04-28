using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.ViewOrchestrators.Api.Host
{
    public class HostFetchOneOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IMapper _mapper;
        private readonly HostRepository _hostRepository;

        public HostFetchOneOrchestrator(IDbContextFactory dbContextFactory, IMapper mapper, HostRepository hostRepository)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _hostRepository = hostRepository;
        }

        public async Task<IActionResult> Execute(string hash)
        {
            int hostId = UrlUtils.Decode(hash);
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var hostEntity = await _hostRepository.GetByIdAsync(dbContext, hostId);
                var result = _mapper.Map<HostEntity, HostViewModel>(hostEntity);
                return new OkObjectResult(result);
            }
        }
    }
}
