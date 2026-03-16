using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Mapping;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.ViewOrchestrators.Api.Job
{
    public interface IJobCreateOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(JobViewModel jobViewModel, string userName);
    }

    public class JobCreateOrchestrator : IJobCreateOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ICreateJobCommand _createJobCommand;

        public JobCreateOrchestrator(IDbContextFactory dbContextFactory, ICreateJobCommand createJobCommand)
        {
            _dbContextFactory = dbContextFactory;
            _createJobCommand = createJobCommand;
        }

        public async Task<IActionResult> Execute(JobViewModel jobViewModel, string userName)
        {
            JobEntity jobEntity = jobViewModel.ToEntity();
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                try
                {
                    dbContext.BeginTransaction();
                    jobEntity = await _createJobCommand.ExecuteAsync(dbContext, jobEntity, userName);
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
