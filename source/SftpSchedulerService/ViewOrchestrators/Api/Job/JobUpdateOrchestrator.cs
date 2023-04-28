using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models;
using SftpSchedulerService.Models.Job;
using System.Data;

namespace SftpSchedulerService.ViewOrchestrators.Api.Job
{
    public class JobUpdateOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IMapper _mapper;
        private readonly IUpdateJobCommand _updateJobCommand;

        public JobUpdateOrchestrator(IDbContextFactory dbContextFactory, IMapper mapper, IUpdateJobCommand updateJobCommand)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _updateJobCommand = updateJobCommand;
        }

        public async Task<IActionResult> Execute(JobViewModel jobViewModel)
        {
            JobEntity jobEntity = _mapper.Map<JobEntity>(jobViewModel);
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                try
                {
                    dbContext.BeginTransaction();
                    jobEntity = await _updateJobCommand.ExecuteAsync(dbContext, jobEntity);
                    dbContext.Commit();
                }
                catch (DataValidationException dve)
                {
                    return new BadRequestObjectResult(dve.ValidationResult);
                }
                jobViewModel.Id = jobEntity.Id;
            }
            var result = _mapper.Map<JobViewModel>(jobEntity);
            return new OkObjectResult(result);
        }
    }
}
