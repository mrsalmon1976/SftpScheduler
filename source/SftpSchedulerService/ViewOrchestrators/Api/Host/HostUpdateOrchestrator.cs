using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models.Host;

namespace SftpSchedulerService.ViewOrchestrators.Api.Host
{
    public interface IHostUpdateOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(HostViewModel hostViewModel, string userName);
    }

    public class HostUpdateOrchestrator : IHostUpdateOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IMapper _mapper;
        private readonly IUpdateHostCommand _updateHostCommand;

        public HostUpdateOrchestrator(IDbContextFactory dbContextFactory, IMapper mapper, IUpdateHostCommand updateHostCommand)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _updateHostCommand = updateHostCommand;
        }

        public async Task<IActionResult> Execute(HostViewModel hostViewModel, string userName)
        {
            HostEntity hostEntity = _mapper.Map<HostEntity>(hostViewModel);
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                try
                {
                    dbContext.BeginTransaction();
                    hostEntity = await _updateHostCommand.ExecuteAsync(dbContext, hostEntity, userName);
                    dbContext.Commit();
                }
                catch (DataValidationException dve)
                {
                    return new BadRequestObjectResult(dve.ValidationResult);
                }
            }
            var result = _mapper.Map<HostViewModel>(hostEntity);
            return new OkObjectResult(result);
        }
    }
}
