﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
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
        private readonly IMapper _mapper;
        private readonly ICreateJobCommand _createJobCommand;

        public JobCreateOrchestrator(IDbContextFactory dbContextFactory, IMapper mapper, ICreateJobCommand createJobCommand)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _createJobCommand = createJobCommand;
        }

        public async Task<IActionResult> Execute(JobViewModel jobViewModel, string userName)
        {
            JobEntity jobEntity = _mapper.Map<JobEntity>(jobViewModel);
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
            var result = _mapper.Map<JobViewModel>(jobEntity);
            return new OkObjectResult(result);
        }
    }
}
