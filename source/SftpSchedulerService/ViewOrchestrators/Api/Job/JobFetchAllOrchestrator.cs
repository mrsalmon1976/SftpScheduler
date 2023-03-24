﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Models;
using SftpSchedulerService.Models.Job;

namespace SftpSchedulerService.ViewOrchestrators.Api.Job
{
    public class JobFetchAllOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IMapper _mapper;
        private readonly JobRepository _jobRepository;
        private readonly ISchedulerFactory _schedulerFactory;

        public JobFetchAllOrchestrator(IDbContextFactory dbContextFactory, IMapper mapper, JobRepository jobRepository, ISchedulerFactory schedulerFactory)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _jobRepository = jobRepository;
            _schedulerFactory = schedulerFactory;
        }

        public async Task<IActionResult> Execute()
        {
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var jobEntities = await _jobRepository.GetAllAsync(dbContext);
                var result = _mapper.Map<JobEntity[], JobViewModel[]>(jobEntities.ToArray());
                var scheduler = await _schedulerFactory.GetScheduler();

                foreach (JobViewModel jobViewModel in result) 
                {
                    TriggerKey triggerKey = new TriggerKey(TransferJob.GetTriggerKeyName(jobViewModel.Id), TransferJob.DefaultGroup);
                    ITrigger? trigger = await scheduler.GetTrigger(triggerKey);
                    if (trigger != null)
                    {
                        jobViewModel.NextRunTime = trigger.GetNextFireTimeUtc()?.LocalDateTime;
                    }
                }

                return new OkObjectResult(result);
            }
        }
    }
}
