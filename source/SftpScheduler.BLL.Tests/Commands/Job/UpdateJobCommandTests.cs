using NSubstitute;
using NUnit.Framework;
using Quartz;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Utility.IO;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using WinSCP;

namespace SftpScheduler.BLL.Tests.Commands.Job
{
    [TestFixture]
    public class UpdateJobCommandTests
    {

        [Test]
        public void Execute_ValidJob_ExecutesQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            var jobEntity = EntityTestHelper.CreateJobEntity();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            UpdateJobCommand updateJobCommand = new UpdateJobCommand(jobValidator, Substitute.For<ISchedulerFactory>());
            updateJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), jobEntity);
        }


        [Test]
        public void Execute_InvalidJob_ThrowsDataValidationException()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            var jobEntity = EntityTestHelper.CreateJobEntity();
            jobValidator.Validate(dbContext, Arg.Any<JobEntity>()).Returns(new ValidationResult(new string[] { "error" }));

            UpdateJobCommand updateJobCommand = new UpdateJobCommand(jobValidator, Substitute.For<ISchedulerFactory>());

            Assert.Throws<DataValidationException>(() => updateJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult());
            jobValidator.Received(1).Validate(dbContext, jobEntity);
        }

        [Test]
        public void Execute_ValidJob_SetsScheduleInWords()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();

            var jobEntity = EntityTestHelper.CreateJobEntity();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            UpdateJobCommand updateJobCommand = new UpdateJobCommand(jobValidator, Substitute.For<ISchedulerFactory>());
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            string expectedScheduleInWords = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(jobEntity.Schedule);
            Assert.That(result.ScheduleInWords, Is.EqualTo(expectedScheduleInWords));
        }

        [TestCase("Test")]
        [TestCase("/Test")]
        [TestCase("Test/")]
        [TestCase("/Test/")]
        public void Execute_ValidJob_SetsRemotePathLeadingTrailingSlash(string remotePath)
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();

            var jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.RemotePath = remotePath;

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            UpdateJobCommand updateJobCommand = new UpdateJobCommand(jobValidator, Substitute.For<ISchedulerFactory>());
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            Assert.That(result.RemotePath, Is.EqualTo("/Test/"));
        }

        [TestCase("Test")]
        [TestCase("/Test")]
        [TestCase("Test/")]
        [TestCase("/Test/")]
        public void Execute_ValidJob_SetsRemoteArchivePathLeadingTrailingSlash(string remoteArchivePath)
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();

            var jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.RemoteArchivePath = remoteArchivePath;

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            UpdateJobCommand updateJobCommand = new UpdateJobCommand(jobValidator, Substitute.For<ISchedulerFactory>());
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            Assert.That(result.RemoteArchivePath, Is.EqualTo("/Test/"));
        }


        [Test]
        public void Execute_ValidJobIsEnabled_SchedulesQuartzJob()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();

            var jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.IsEnabled = true;

            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            // execute
            UpdateJobCommand updateJobCommand = new UpdateJobCommand(jobValidator, schedulerFactory);
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            // assert
            schedulerFactory.Received(1).GetScheduler();
            scheduler.Received(1).ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>());
        }

        [Test]
        public void Execute_ValidJobScheduleExists_ScheduledIsDeleted()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();

            var jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.IsEnabled = true;

            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);
            scheduler.CheckExists(Arg.Any<JobKey>()).Returns(true);

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            // execute
            UpdateJobCommand updateJobCommand = new UpdateJobCommand(jobValidator, schedulerFactory);
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            // assert
            scheduler.Received(1).CheckExists(Arg.Any<JobKey>());
            scheduler.Received(1).DeleteJob(Arg.Any<JobKey>());
        }

        [Test]
        public void Execute_ValidJobScheduleDoesNotExist_DeleteJobNotCalled()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();

            var jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.IsEnabled = true;

            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);
            scheduler.CheckExists(Arg.Any<JobKey>()).Returns(false);

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            // execute
            UpdateJobCommand updateJobCommand = new UpdateJobCommand(jobValidator, schedulerFactory);
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            // assert
            scheduler.Received(1).CheckExists(Arg.Any<JobKey>());
            scheduler.DidNotReceive().DeleteJob(Arg.Any<JobKey>());
        }


        [Test]
        public void Execute_ValidJobIsNotEnabled_DoesNotScheduleQuartzJob()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();
            IScheduler scheduler = Substitute.For<IScheduler>();

            var jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.IsEnabled = false;

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            // execute
            UpdateJobCommand updateJobCommand = new UpdateJobCommand(jobValidator, schedulerFactory);
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            // assert
            scheduler.DidNotReceive().ScheduleJob(Arg.Any<IJobDetail>(),  Arg.Any<ITrigger>());
        }


        [Test]
        public void Execute_ValidJob_ValidTriggerCreated()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();

            var jobEntity = EntityTestHelper.CreateJobEntity();

            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);
            scheduler.When(x => x.ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>())).Do((c) =>
            {
                ITrigger trigger = c.ArgAt<ITrigger>(1);
                Assert.That(trigger.Key.Group, Is.EqualTo($"{TransferJob.DefaultGroup}"));
                Assert.That(trigger.Key.Name, Is.EqualTo($"Trigger.{jobEntity.Id}"));
            });


            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            // execute
            UpdateJobCommand updateJobCommand = new UpdateJobCommand(jobValidator, schedulerFactory);
            updateJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();
        }

        [Test]
        public void Execute_ValidJob_ValidJobCreated()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();

            var jobEntity = EntityTestHelper.CreateJobEntity();

            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);
            scheduler.When(x => x.ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>())).Do((c) =>
            {
                IJobDetail jobDetail = c.ArgAt<IJobDetail>(0);
                Assert.That(jobDetail.Key.Group, Is.EqualTo($"{TransferJob.DefaultGroup}"));
                Assert.That(jobDetail.Key.Name, Is.EqualTo($"Job.{jobEntity.Id}"));
            });


            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            // execute
            UpdateJobCommand updateJobCommand = new UpdateJobCommand(jobValidator, schedulerFactory);
            updateJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();
        }


        [Test]
        public void ExecuteAsync_Integration_ExecutesQueryWithoutError()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    UpdateJobCommand updateJobCommand = new UpdateJobCommand(new JobValidator(new HostRepository(), new DirectoryWrap()), Substitute.For<ISchedulerFactory>());
                    HostEntity host = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    DateTime dtBefore = DateTime.UtcNow;
                    JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
                    jobEntity.LocalPath = AppDomain.CurrentDomain.BaseDirectory;
                    jobEntity.HostId = host.Id;

                    JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

                    Assert.IsNotNull(result);
                    Assert.That(result.Name, Is.EqualTo(jobEntity.Name));
                    Assert.That(jobEntity.Created, Is.GreaterThanOrEqualTo(dtBefore));

                }
            }
        }


    }
}
