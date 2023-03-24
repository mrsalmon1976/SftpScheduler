using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Quartz;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Job
{
    [TestFixture]
    public class JobFetchAllOrchestratorTests
    {
        [Test]
        public void Execute_OnFetch_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            JobRepository jobRepo = Substitute.For<JobRepository>();

            JobViewModel[] jobViewModels = { ViewModelTestHelper.CreateJobViewModel() };
            JobEntity[] jobEntities = { EntityTestHelper.CreateJobEntity() };

            jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(jobEntities);
            mapper.Map<JobEntity[], JobViewModel[]>(Arg.Any<JobEntity[]>()).Returns(jobViewModels);


            JobFetchAllOrchestrator jobFetchAllProvider = new JobFetchAllOrchestrator(dbContextFactory, mapper, jobRepo, Substitute.For<ISchedulerFactory>());
            var result = jobFetchAllProvider.Execute().Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Not.Null);

            IEnumerable<JobViewModel> jobViewModelResult = (IEnumerable<JobViewModel>)result.Value;

            Assert.IsNotNull(jobViewModelResult);
            Assert.That(jobViewModelResult.Count, Is.EqualTo(1));
        }

        [Test]
        public void Execute_JobsFound_HydratesNextRunTime()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            JobRepository jobRepo = Substitute.For<JobRepository>();

            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();
            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);

            JobViewModel jobViewModel = ViewModelTestHelper.CreateJobViewModel();
            jobViewModel.NextRunTime = null;
            TriggerKey triggerKey = new TriggerKey(TransferJob.GetTriggerKeyName(jobViewModel.Id), TransferJob.DefaultGroup);

            DateTime nextFireTime = DateTime.UtcNow.AddSeconds(Faker.RandomNumber.Next(1, 1000));
            ITrigger trigger = Substitute.For<ITrigger>();
            scheduler.GetTrigger(triggerKey).Returns(trigger);
            trigger.GetNextFireTimeUtc().Returns(nextFireTime);

            JobViewModel[] jobViewModels = { jobViewModel };
            JobEntity[] jobEntities = { EntityTestHelper.CreateJobEntity() };

            jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(jobEntities);
            mapper.Map<JobEntity[], JobViewModel[]>(Arg.Any<JobEntity[]>()).Returns(jobViewModels);


            // execute
            JobFetchAllOrchestrator jobFetchAllProvider = new JobFetchAllOrchestrator(dbContextFactory, mapper, jobRepo, schedulerFactory);
            var result = jobFetchAllProvider.Execute().Result as OkObjectResult;

            JobViewModel jobViewModelResult = ((IEnumerable<JobViewModel>)result.Value).Single();


            Assert.That(jobViewModelResult.NextRunTime, Is.EqualTo(nextFireTime.ToLocalTime()));
        }

    }
}
