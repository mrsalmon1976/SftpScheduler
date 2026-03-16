using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.ViewOrchestrators.Api.Job
{
    public interface IJobUpdateOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(JobViewModel jobViewModel, string userName);
    }

    public class JobUpdateOrchestrator : IJobUpdateOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IUpdateJobCommand _updateJobCommand;

        public JobUpdateOrchestrator(IDbContextFactory dbContextFactory, IUpdateJobCommand updateJobCommand)
        {
            _dbContextFactory = dbContextFactory;
            _updateJobCommand = updateJobCommand;
        }

        public async Task<IActionResult> Execute(JobViewModel jobViewModel, string userName)
        {
            JobEntity jobEntity = jobViewModel.ToEntity();
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                try
                {
                    dbContext.BeginTransaction();
                    jobEntity = await _updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName);
                    dbContext.Commit();
                }
                catch (DataValidationException dve)
                {
                    return new BadRequestObjectResult(dve.ValidationResult);
                }
                jobViewModel.Id = jobEntity.Id;
            }
            var result = jobEntity.ToViewModel();
            return new OkObjectResult(result);
        }
    }
}
