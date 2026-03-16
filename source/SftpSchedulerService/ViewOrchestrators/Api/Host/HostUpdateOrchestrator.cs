using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Mapping;
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
        private readonly IUpdateHostCommand _updateHostCommand;

        public HostUpdateOrchestrator(IDbContextFactory dbContextFactory, IUpdateHostCommand updateHostCommand)
        {
            _dbContextFactory = dbContextFactory;
            _updateHostCommand = updateHostCommand;
        }

        public async Task<IActionResult> Execute(HostViewModel hostViewModel, string userName)
        {
            HostEntity hostEntity = hostViewModel.ToEntity();
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
            var result = hostEntity.ToViewModel();
            return new OkObjectResult(result);
        }
    }
}
