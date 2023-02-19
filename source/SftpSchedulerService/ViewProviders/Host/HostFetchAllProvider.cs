using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Queries;
using SftpSchedulerService.Models;

namespace SftpSchedulerService.ViewProviders.Host
{
    public class HostFetchAllProvider
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IMapper _mapper;
        private readonly HostRepository _hostRepository;

        public HostFetchAllProvider(IDbContextFactory dbContextFactory, IMapper mapper, HostRepository hostRepository)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _hostRepository = hostRepository;
        }

        public async Task<IActionResult> Execute()
        {
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var hostEntities = await _hostRepository.GetAllAsync(dbContext);
                var result = _mapper.Map<HostEntity[], HostViewModel[]>(hostEntities.ToArray());
                return new OkObjectResult(result);
            }
        }
    }
}
