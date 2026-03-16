using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.Host;

namespace SftpSchedulerService.ViewOrchestrators.Api.Host
{
    public interface IHostCreateOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(HostViewModel hostViewModel, string userName);
    }

    public class HostCreateOrchestrator : IHostCreateOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ICreateHostCommand _createHostCommand;

        public HostCreateOrchestrator(IDbContextFactory dbContextFactory, ICreateHostCommand createHostCommand)
        {
            _dbContextFactory = dbContextFactory;
            _createHostCommand = createHostCommand;
        }

        public async Task<IActionResult> Execute(HostViewModel hostViewModel, string userName)
        {
            HostEntity hostEntity = hostViewModel.ToEntity();
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                try
                {
                    dbContext.BeginTransaction();
                    hostEntity = await _createHostCommand.ExecuteAsync(dbContext, hostEntity, userName);
                    dbContext.Commit();
                }
                catch (DataValidationException dve)
                {
                    return new BadRequestObjectResult(dve.ValidationResult);
                }
                hostViewModel.Id = hostEntity.Id;
            }
            var result = hostEntity.ToViewModel();
            return new OkObjectResult(result);
        }
    }
}
