using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.ViewOrchestrators.Api.Job
{
    public class JobCreateOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IMapper _mapper;
        private readonly CreateJobCommand _createJobCommand;

        public JobCreateOrchestrator(IDbContextFactory dbContextFactory, IMapper mapper, CreateJobCommand createJobCommand)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _createJobCommand = createJobCommand;
        }

        public async Task<IActionResult> Execute(JobViewModel jobViewModel)
        {
            JobEntity jobEntity = _mapper.Map<JobEntity>(jobViewModel);
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                try
                {
                    jobEntity = await _createJobCommand.ExecuteAsync(dbContext, jobEntity);
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
