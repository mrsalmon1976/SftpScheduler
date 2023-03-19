﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Models;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.ViewOrchestrators.Api.Job
{
    public class JobFetchOneOrchestrator
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IMapper _mapper;
        private readonly JobRepository _jobRepository;

        public JobFetchOneOrchestrator(IDbContextFactory dbContextFactory, IMapper mapper, JobRepository jobRepository)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _jobRepository = jobRepository;
        }

        public async Task<IActionResult> Execute(string hash)
        {
            int jobId = UrlUtils.Decode(hash);
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var jobEntity = await _jobRepository.GetByIdAsync(dbContext, jobId);
                var result = _mapper.Map<JobEntity, JobViewModel>(jobEntity);
                return new OkObjectResult(result);
            }
        }
    }
}
