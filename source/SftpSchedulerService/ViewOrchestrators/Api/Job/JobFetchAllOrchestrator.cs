using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
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

        public JobFetchAllOrchestrator(IDbContextFactory dbContextFactory, IMapper mapper, JobRepository jobRepository)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _jobRepository = jobRepository;
        }

        public async Task<IActionResult> Execute()
        {
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                var jobEntities = await _jobRepository.GetAllAsync(dbContext);
                var result = _mapper.Map<JobEntity[], JobViewModel[]>(jobEntities.ToArray());
                return new OkObjectResult(result);
            }
        }
    }
}
