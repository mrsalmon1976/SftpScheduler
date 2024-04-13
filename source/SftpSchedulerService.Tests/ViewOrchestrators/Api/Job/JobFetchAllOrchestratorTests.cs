using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Quartz;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Models.Job;
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

            JobViewModel[] jobViewModels = { new SubstituteBuilder<JobViewModel>().WithRandomProperties().Build()        };
            JobEntity[] jobEntities = { new SubstituteBuilder<JobEntity>().WithRandomProperties().Build() };

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

            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().WithProperty(x => x.IsEnabled, true).Build();
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
        public void Execute_JobsFound_HydratesIsFailing()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = AutoMapperTestHelper.CreateMapper();
            JobRepository jobRepo = Substitute.For<JobRepository>();

            JobEntity jobEntitySuccess = new SubstituteBuilder<JobEntity>().WithRandomProperties().WithProperty(x => x.Id, 1).Build();
            JobEntity jobEntityFailing = new SubstituteBuilder<JobEntity>().WithRandomProperties().WithProperty(x => x.Id, 2).Build();
            JobEntity[] jobEntities = { jobEntitySuccess, jobEntityFailing };
            JobEntity[] failingJobEntities = { jobEntityFailing };

            jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(jobEntities);
            jobRepo.GetAllFailingAsync(Arg.Any<IDbContext>()).Returns(failingJobEntities);

            // execute
            JobFetchAllOrchestrator jobFetchAllProvider = new JobFetchAllOrchestrator(dbContextFactory, mapper, jobRepo, Substitute.For<ISchedulerFactory>());
            var result = jobFetchAllProvider.Execute().Result as OkObjectResult;

            List<JobViewModel> jobViewModelResult = ((IEnumerable<JobViewModel>)result.Value).ToList();


            // assert
            jobRepo.Received(1).GetAllFailingAsync(Arg.Any<IDbContext>()).GetAwaiter().GetResult();
            Assert.That(jobViewModelResult[0].IsFailing, Is.False);
            Assert.That(jobViewModelResult[1].IsFailing, Is.True);
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

            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().WithProperty(x => x.IsEnabled, false).Build();
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
