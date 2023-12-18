﻿using NSubstitute;
using NUnit.Framework;
using Quartz;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.Common.IO;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Validators;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpScheduler.BLL.Services.Job;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Services.Host;
using SftpScheduler.BLL.Tests.Builders.Data;
using SftpScheduler.BLL.Tests.Builders.Services.Host;
using SftpScheduler.BLL.Tests.Builders.Services.Job;

namespace SftpScheduler.BLL.Tests.Commands.Job
{
    [TestFixture]
    public class UpdateJobCommandTests
    {

        [Test]
        public void Execute_ValidJob_ExecutesQuery()
        {
            // setup
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            var jobEntity = new JobEntityBuilder().WithRandomProperties().Build();
            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            string userName = Guid.NewGuid().ToString();

            UpdateJobCommand updateJobCommand = CreateCommand(jobValidator: jobValidator);
            updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), jobEntity);
        }


        [Test]
        public void Execute_InvalidJob_ThrowsDataValidationException()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            var jobEntity = new JobEntityBuilder().WithRandomProperties().Build();
            jobValidator.Validate(dbContext, Arg.Any<JobEntity>()).Returns(new ValidationResult(new string[] { "error" }));
            string userName = Guid.NewGuid().ToString();

            // execute
            UpdateJobCommand updateJobCommand = CreateCommand(jobValidator: jobValidator);

            Assert.Throws<DataValidationException>(() => updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult());
            jobValidator.Received(1).Validate(dbContext, jobEntity);
        }

        [Test]
        public void Execute_ValidJob_SetsScheduleInWords()
        {
            // setup
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();

            var jobEntity = new JobEntityBuilder().WithRandomProperties().Build();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            string userName = Guid.NewGuid().ToString();

            // execute
            UpdateJobCommand updateJobCommand = CreateCommand(jobValidator: jobValidator);
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult();

            // assert
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

            var jobEntity = new JobEntityBuilder().WithRandomProperties().WithRemotePath(remotePath).Build();
            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            string userName = Guid.NewGuid().ToString();

            UpdateJobCommand updateJobCommand = CreateCommand(jobValidator: jobValidator);
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult();

            Assert.That(result.RemotePath, Is.EqualTo("/Test/"));
        }

        [Test]
        public void Execute_ValidJobWithMinimalRemotePath_DoesNotSetLeadingTrailingSlash()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();

            var jobEntity = new JobEntityBuilder().WithRandomProperties().WithRemotePath("/").Build();
            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            string userName = Guid.NewGuid().ToString();

            UpdateJobCommand updateJobCommand = CreateCommand(jobValidator: jobValidator);
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult();

            Assert.That(result.RemotePath, Is.EqualTo("/"));
        }


        [TestCase("Test")]
        [TestCase("/Test")]
        [TestCase("Test/")]
        [TestCase("/Test/")]
        public void Execute_ValidJob_SetsRemoteArchivePathLeadingTrailingSlash(string remoteArchivePath)
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();

            var jobEntity = new JobEntityBuilder().WithRandomProperties().WithRemoteArchivePath(remoteArchivePath).Build();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            string userName = Guid.NewGuid().ToString();

            UpdateJobCommand updateJobCommand = CreateCommand(jobValidator);
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult();

            Assert.That(result.RemoteArchivePath, Is.EqualTo("/Test/"));
        }

        [Test]
        public void Execute_ValidJobWithMinimalRemoteArchivePath_DoesNotSetLeadingTrailingSlash()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();

            var jobEntity = new JobEntityBuilder().WithRandomProperties().WithRemoteArchivePath("/").Build();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            string userName = Guid.NewGuid().ToString();

            UpdateJobCommand updateJobCommand = CreateCommand(jobValidator);
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult();

            Assert.That(result.RemoteArchivePath, Is.EqualTo("/"));
        }


        [Test]
        public void Execute_ValidJobIsEnabled_SchedulesQuartzJob()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();

            var jobEntity = new JobEntityBuilder().WithRandomProperties().WithIsEnabled(true).Build();

            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            string userName = Guid.NewGuid().ToString();

            // execute
            UpdateJobCommand updateJobCommand = CreateCommand(jobValidator, schedulerFactory);
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult();

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

            var jobEntity = new JobEntityBuilder().WithRandomProperties().WithIsEnabled(true).Build();

            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);
            scheduler.CheckExists(Arg.Any<JobKey>()).Returns(true);

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            string userName = Guid.NewGuid().ToString();

            // execute
            UpdateJobCommand updateJobCommand = CreateCommand(jobValidator, schedulerFactory);
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult();

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

            var jobEntity = new JobEntityBuilder().WithRandomProperties().WithIsEnabled(true).Build();

            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);
            scheduler.CheckExists(Arg.Any<JobKey>()).Returns(false);

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            string userName = Guid.NewGuid().ToString();

            // execute
            UpdateJobCommand updateJobCommand = CreateCommand(jobValidator, schedulerFactory);
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult();

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

            var jobEntity = new JobEntityBuilder().WithRandomProperties().WithIsEnabled(false).Build();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            string userName = Guid.NewGuid().ToString();

            // execute
            UpdateJobCommand updateJobCommand = CreateCommand(jobValidator, schedulerFactory);
            JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult();

            // assert
            scheduler.DidNotReceive().ScheduleJob(Arg.Any<IJobDetail>(),  Arg.Any<ITrigger>());
        }


        [Test]
        public void Execute_ValidJob_ValidTriggerCreated()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();

            var jobEntity = new JobEntityBuilder().WithRandomProperties().Build();

            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);
            scheduler.When(x => x.ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>())).Do((c) =>
            {
                ITrigger trigger = c.ArgAt<ITrigger>(1);
                Assert.That(trigger.Key.Group, Is.EqualTo($"{TransferJob.GroupName}"));
                Assert.That(trigger.Key.Name, Is.EqualTo($"TransferTrigger.{jobEntity.Id}"));
            });


            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            string userName = Guid.NewGuid().ToString();

            // execute
            UpdateJobCommand updateJobCommand = CreateCommand(jobValidator, schedulerFactory);
            updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult();
        }

        [Test]
        public void Execute_ValidJob_JobUpdated()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();

            var jobEntity = new JobEntityBuilder().WithRandomProperties().Build();

            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);
            scheduler.When(x => x.ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>())).Do((c) =>
            {
                IJobDetail jobDetail = c.ArgAt<IJobDetail>(0);
                Assert.That(jobDetail.Key.Group, Is.EqualTo($"{TransferJob.GroupName}"));
                Assert.That(jobDetail.Key.Name, Is.EqualTo($"TransferJob.{jobEntity.Id}"));
            });


            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            string userName = Guid.NewGuid().ToString();

            // execute
            UpdateJobCommand updateJobCommand = CreateCommand(jobValidator, schedulerFactory: schedulerFactory);
            updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult();
        }

        [Test]
        public void Execute_ValidJob_CreatesAuditLogs()
        {
            // setup 
            IDbContext dbContext = new DbContextBuilder().Build();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            var jobEntity = new JobEntityBuilder().WithRandomProperties().Build();
            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            string userName = Guid.NewGuid().ToString();

            JobAuditLogEntity jobAuditLogEntity1 = new JobAuditLogEntityBuilder().WithRandomProperties().WithJobId(jobEntity.Id).Build();
            JobAuditLogEntity jobAuditLogEntity2 = new JobAuditLogEntityBuilder().WithRandomProperties().WithJobId(jobEntity.Id).Build();
            IEnumerable<JobAuditLogEntity> auditLogEntities = new JobAuditLogEntity[] { jobAuditLogEntity1, jobAuditLogEntity2 };

            IJobAuditService jobAuditService = new JobAuditServiceBuilder()
                .WithCompareHostsReturns(dbContext, Arg.Any<JobEntity>(), Arg.Any<JobEntity>(), userName, auditLogEntities)
                .Build();

            // execute
            UpdateJobCommand command = CreateCommand(jobValidator: jobValidator, jobAuditService: jobAuditService);
            command.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult();

            // assert
            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), jobEntity);
            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), jobAuditLogEntity1);
            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), jobAuditLogEntity2);

        }



        [Test]
        public void ExecuteAsync_Integration_ExecutesQueryWithoutError()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                string userName = Guid.NewGuid().ToString();
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    JobRepository jobRepo = new JobRepository();
                    HostRepository hostRepo = new HostRepository();
                    IJobValidator jobValidator = new JobValidator(hostRepo, new DirectoryUtility());
                    IJobAuditService jobAuditService = new JobAuditService(hostRepo);
                        
                    UpdateJobCommand updateJobCommand = CreateCommand(jobValidator, Substitute.For<ISchedulerFactory>(), jobAuditService, jobRepo);
                    HostEntity host = dbIntegrationTestHelper.CreateHostEntity(dbContext);
                    JobEntity job = dbIntegrationTestHelper.CreateJobEntity(dbContext, host.Id);

                    DateTime dtBefore = DateTime.UtcNow;
                    var jobEntity = new JobEntityBuilder()
                        .WithRandomProperties()
                        .WithId(job.Id)
                        .WithHostId(host.Id)
                        .WithLocalPath(AppDomain.CurrentDomain.BaseDirectory)
                        .Build();

                    JobEntity result = updateJobCommand.ExecuteAsync(dbContext, jobEntity, userName).GetAwaiter().GetResult();

                    Assert.IsNotNull(result);
                    Assert.That(result.Name, Is.EqualTo(jobEntity.Name));
                    Assert.That(result.RestartOnFailure, Is.EqualTo(jobEntity.RestartOnFailure));
                    Assert.That(jobEntity.Created, Is.GreaterThanOrEqualTo(dtBefore));

                }
            }
        }

        private UpdateJobCommand CreateCommand(IJobValidator? jobValidator = null
            , ISchedulerFactory? schedulerFactory = null
            , IJobAuditService? jobAuditService = null
            , JobRepository? jobRepo = null
            )
        {
            jobValidator ??= Substitute.For<IJobValidator>();
            schedulerFactory ??= Substitute.For<ISchedulerFactory>();
            jobAuditService ??= Substitute.For<IJobAuditService>();
            jobRepo ??= Substitute.For<JobRepository>();
            return new UpdateJobCommand(jobValidator, schedulerFactory, jobAuditService, jobRepo);
        }


    }
}
