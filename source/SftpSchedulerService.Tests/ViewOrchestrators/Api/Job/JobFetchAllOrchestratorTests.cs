using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Quartz;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.Tests.Builders.Models.Job;
using SftpSchedulerService.ViewOrchestrators.Api.Job;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Job
{
    [TestFixture]
    public class JobFetchAllOrchestratorTests
    {
        [Test]
        public void Execute_OnFetch_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = AutoMapperTestHelper.CreateMapper();
            JobRepository jobRepo = Substitute.For<JobRepository>();

            JobViewModel[] jobViewModels = { new JobViewModelBuilder().WithRandomProperties().Build()        };
            JobEntity[] jobEntities = { new JobEntityBuilder().WithRandomProperties().Build() };

            jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(jobEntities);

            JobFetchAllOrchestrator jobFetchAllProvider = new JobFetchAllOrchestrator(dbContextFactory, mapper, jobRepo, Substitute.For<ISchedulerFactory>());
            var result = jobFetchAllProvider.Execute().Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Not.Null);

            IEnumerable<JobViewModel> jobViewModelResult = (IEnumerable<JobViewModel>)result.Value;

            Assert.IsNotNull(jobViewModelResult);
            Assert.That(jobViewModelResult.Count, Is.EqualTo(1));
        }

        [Test]
        public void Execute_JobsFoundAndEnabled_HydratesNextRunTime()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = AutoMapperTestHelper.CreateMapper();
            JobRepository jobRepo = Substitute.For<JobRepository>();

            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();
            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().WithIsEnabled(true).Build();
            JobEntity[] jobEntities = { jobEntity };

            TriggerKey triggerKey = new TriggerKey(TransferJob.TriggerKeyName(jobEntity.Id), TransferJob.GroupName);

            DateTime nextFireTime = DateTime.UtcNow.AddSeconds(Faker.RandomNumber.Next(1, 1000));
            ITrigger trigger = Substitute.For<ITrigger>();
            scheduler.GetTrigger(triggerKey).Returns(trigger);
            trigger.GetNextFireTimeUtc().Returns(nextFireTime);

            jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(jobEntities);

            // execute
            JobFetchAllOrchestrator jobFetchAllProvider = new JobFetchAllOrchestrator(dbContextFactory, mapper, jobRepo, schedulerFactory);
            var result = jobFetchAllProvider.Execute().Result as OkObjectResult;

            JobViewModel jobViewModelResult = ((IEnumerable<JobViewModel>)result.Value).Single();


            Assert.That(jobViewModelResult.NextRunTime, Is.EqualTo(nextFireTime.ToLocalTime()));
        }

        [Test]
        public void Execute_JobsFoundAndDisabled_TriggerNotLoaded()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = AutoMapperTestHelper.CreateMapper();
            JobRepository jobRepo = Substitute.For<JobRepository>();

            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();
            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().WithIsEnabled(false).Build();
            JobEntity[] jobEntities = { jobEntity };

            jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(jobEntities);

            // execute
            JobFetchAllOrchestrator jobFetchAllProvider = new JobFetchAllOrchestrator(dbContextFactory, mapper, jobRepo, schedulerFactory);
            var result = jobFetchAllProvider.Execute().Result as OkObjectResult;

            JobViewModel jobViewModelResult = ((IEnumerable<JobViewModel>)result.Value).Single();

            scheduler.DidNotReceive().GetTrigger(Arg.Any<TriggerKey>());

            Assert.That(jobViewModelResult.NextRunTime, Is.Null);
        }


    }
}
