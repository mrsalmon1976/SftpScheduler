using Microsoft.AspNetCore.Mvc;
using NSubstitute;
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
        private IDbContextFactory _dbContextFactory = null!;
        private JobRepository _jobRepo = null!;
        private QuartzTriggerRepository _quartzTriggerRepo = null!;

        [SetUp]
        public void SetUp()
        {
            _dbContextFactory = Substitute.For<IDbContextFactory>();
            _jobRepo = Substitute.For<JobRepository>();
            _quartzTriggerRepo = Substitute.For<QuartzTriggerRepository>();
        }

        private JobFetchAllOrchestrator CreateOrchestrator()
        {
            return new JobFetchAllOrchestrator(_dbContextFactory, _jobRepo, _quartzTriggerRepo);
        }

        private void SetTriggers(params QuartzTriggerEntity[] triggers)
        {
            _quartzTriggerRepo.GetAllByGroupAsync(Arg.Any<IDbContext>(), QuartzConstants.SchedulerName, TransferJob.GroupName).Returns(triggers);
        }

        [Test]
        public void Execute_OnFetch_ReturnsOk()
        {
            JobEntity[] jobEntities = { new SubstituteBuilder<JobEntity>().WithRandomProperties().Build() };
            _jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(jobEntities);

            var result = CreateOrchestrator().Execute().Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Not.Null);

            IEnumerable<JobViewModel> jobViewModelResult = (IEnumerable<JobViewModel>)result.Value;

            Assert.IsNotNull(jobViewModelResult);
            Assert.That(jobViewModelResult.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Execute_JobsFoundAndEnabled_HydratesNextRunTime()
        {
            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().WithProperty(x => x.IsEnabled, true).Build();
            _jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(new[] { jobEntity });

            DateTime nextFireTime = DateTime.UtcNow.AddSeconds(Faker.RandomNumber.Next(1, 1000));
            SetTriggers(new QuartzTriggerEntity { TriggerName = TransferJob.TriggerKeyName(jobEntity.Id), NextFireTimeTicks = nextFireTime.Ticks });

            var result = CreateOrchestrator().Execute().Result as OkObjectResult;
            JobViewModel jobViewModelResult = ((IEnumerable<JobViewModel>)result.Value).Single();

            Assert.That(jobViewModelResult.NextRunTime, Is.EqualTo(nextFireTime.ToLocalTime()));
        }

        [Test]
        public void Execute_JobsFoundAndEnabled_LoadsAllTriggersInOneQuery()
        {
            JobEntity jobEntity1 = new SubstituteBuilder<JobEntity>().WithRandomProperties().WithProperty(x => x.Id, 1).WithProperty(x => x.IsEnabled, true).Build();
            JobEntity jobEntity2 = new SubstituteBuilder<JobEntity>().WithRandomProperties().WithProperty(x => x.Id, 2).WithProperty(x => x.IsEnabled, true).Build();
            _jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(new[] { jobEntity1, jobEntity2 });

            SetTriggers(
                new QuartzTriggerEntity { TriggerName = TransferJob.TriggerKeyName(1), NextFireTimeTicks = DateTime.UtcNow.AddHours(1).Ticks },
                new QuartzTriggerEntity { TriggerName = TransferJob.TriggerKeyName(2), NextFireTimeTicks = DateTime.UtcNow.AddHours(2).Ticks });

            var result = CreateOrchestrator().Execute().Result as OkObjectResult;
            List<JobViewModel> jobViewModelResult = ((IEnumerable<JobViewModel>)result.Value).ToList();

            _quartzTriggerRepo.Received(1).GetAllByGroupAsync(Arg.Any<IDbContext>(), QuartzConstants.SchedulerName, TransferJob.GroupName);
            Assert.That(jobViewModelResult[0].NextRunTime, Is.Not.Null);
            Assert.That(jobViewModelResult[1].NextRunTime, Is.Not.Null);
        }

        [Test]
        public void Execute_JobEnabledButNoTriggerRow_NextRunTimeIsNull()
        {
            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().WithProperty(x => x.IsEnabled, true).Build();
            _jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(new[] { jobEntity });
            SetTriggers();

            var result = CreateOrchestrator().Execute().Result as OkObjectResult;
            JobViewModel jobViewModelResult = ((IEnumerable<JobViewModel>)result.Value).Single();

            Assert.That(jobViewModelResult.NextRunTime, Is.Null);
        }

        [Test]
        public void Execute_TriggerHasNoNextFireTime_NextRunTimeIsNull()
        {
            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().WithProperty(x => x.IsEnabled, true).Build();
            _jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(new[] { jobEntity });
            SetTriggers(new QuartzTriggerEntity { TriggerName = TransferJob.TriggerKeyName(jobEntity.Id), NextFireTimeTicks = null });

            var result = CreateOrchestrator().Execute().Result as OkObjectResult;
            JobViewModel jobViewModelResult = ((IEnumerable<JobViewModel>)result.Value).Single();

            Assert.That(jobViewModelResult.NextRunTime, Is.Null);
        }

        [Test]
        public void Execute_JobsFoundAndDisabled_NextRunTimeIsNull()
        {
            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().WithProperty(x => x.IsEnabled, false).Build();
            _jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(new[] { jobEntity });

            // Even if a trigger row exists for a disabled job, its next run time must not be shown.
            SetTriggers(new QuartzTriggerEntity { TriggerName = TransferJob.TriggerKeyName(jobEntity.Id), NextFireTimeTicks = DateTime.UtcNow.AddHours(1).Ticks });

            var result = CreateOrchestrator().Execute().Result as OkObjectResult;
            JobViewModel jobViewModelResult = ((IEnumerable<JobViewModel>)result.Value).Single();

            Assert.That(jobViewModelResult.NextRunTime, Is.Null);
        }

        [Test]
        public void Execute_JobsFound_HydratesIsFailing()
        {
            JobEntity jobEntitySuccess = new SubstituteBuilder<JobEntity>().WithRandomProperties().WithProperty(x => x.Id, 1).Build();
            JobEntity jobEntityFailing = new SubstituteBuilder<JobEntity>().WithRandomProperties().WithProperty(x => x.Id, 2).Build();
            JobEntity[] jobEntities = { jobEntitySuccess, jobEntityFailing };
            JobEntity[] failingJobEntities = { jobEntityFailing };

            _jobRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(jobEntities);
            _jobRepo.GetAllFailingAsync(Arg.Any<IDbContext>()).Returns(failingJobEntities);

            var result = CreateOrchestrator().Execute().Result as OkObjectResult;
            List<JobViewModel> jobViewModelResult = ((IEnumerable<JobViewModel>)result.Value).ToList();

            _jobRepo.Received(1).GetAllFailingAsync(Arg.Any<IDbContext>()).GetAwaiter().GetResult();
            Assert.That(jobViewModelResult[0].IsFailing, Is.False);
            Assert.That(jobViewModelResult[1].IsFailing, Is.True);
        }
    }
}
