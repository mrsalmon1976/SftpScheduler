using NSubstitute;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using SftpScheduler.BLL.Tests.Builders.Models;

namespace SftpScheduler.BLL.Tests.Commands.Job
{
    [TestFixture]
    public class CreateJobCommandTests
    {

        [Test]
        public void Execute_ValidJob_ExecutesQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().Build();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            ICreateJobCommand createJobCommand = new CreateJobCommand(jobValidator, Substitute.For<ISchedulerFactory>());
            createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), jobEntity);
            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());

        }


        [Test]
        public void Execute_InvalidJob_ThrowsDataValidationException()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().Build();
            jobValidator.Validate(dbContext, Arg.Any<JobEntity>()).Returns(new ValidationResult(new string[] { "error" }));

            ICreateJobCommand createJobCommand = new CreateJobCommand(jobValidator, Substitute.For<ISchedulerFactory>());

            Assert.Throws<DataValidationException>(() => createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult());
            jobValidator.Received(1).Validate(dbContext, jobEntity);
        }

        [Test]
        public void Execute_ValidJob_PopulatesJobIdOnReturnValue()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().Build();
            int newEntityId = Faker.RandomNumber.Next();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());
            dbContext.ExecuteScalarAsync<int>(Arg.Any<string>()).Returns(newEntityId);

            ICreateJobCommand createJobCommand = new CreateJobCommand(jobValidator, Substitute.For<ISchedulerFactory>());
            JobEntity result = createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());
            Assert.That(result.Id, Is.EqualTo(newEntityId));
        }

        [Test]
        public void Execute_ValidJob_SetsScheduleInWords()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().Build();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            ICreateJobCommand createJobCommand = new CreateJobCommand(jobValidator, Substitute.For<ISchedulerFactory>());
            JobEntity result = createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());

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

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().WithRemotePath(remotePath).Build();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            ICreateJobCommand createJobCommand = new CreateJobCommand(jobValidator, Substitute.For<ISchedulerFactory>());
            JobEntity result = createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());

            Assert.That(result.RemotePath, Is.EqualTo("/Test/"));
        }

        [Test]
        public void Execute_ValidJobWithMinimalRemotePath_DoesNotSetLeadingTrailingSlash()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().WithRemotePath("/").Build();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            ICreateJobCommand createJobCommand = new CreateJobCommand(jobValidator, Substitute.For<ISchedulerFactory>());
            JobEntity result = createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());

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

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().WithRemoteArchivePath(remoteArchivePath).Build();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            ICreateJobCommand createJobCommand = new CreateJobCommand(jobValidator, Substitute.For<ISchedulerFactory>());
            JobEntity result = createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());

            Assert.That(result.RemoteArchivePath, Is.EqualTo("/Test/"));
        }


        [Test]
        public void Execute_ValidJobWithMinimalRemoteArchivePath_DoesNotSetLeadingTrailingSlash()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().WithRemoteArchivePath("/").Build();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            ICreateJobCommand createJobCommand = new CreateJobCommand(jobValidator, Substitute.For<ISchedulerFactory>());
            JobEntity result = createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());

            Assert.That(result.RemoteArchivePath, Is.EqualTo("/"));
        }


        [Test]
        public void Execute_ValidJobIsEnabled_SchedulesQuartzJob()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().WithIsEnabled(true).Build();

            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            // execute
            ICreateJobCommand createJobCommand = new CreateJobCommand(jobValidator, schedulerFactory);
            JobEntity result = createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            // assert
            schedulerFactory.Received(1).GetScheduler();
            scheduler.Received(1).ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>());
        }

        [Test]
        public void Execute_ValidJobIsNotEnabled_DoesNotScheduleQuartzJob()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();
            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().WithIsEnabled(false).Build();

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            // execute
            ICreateJobCommand createJobCommand = new CreateJobCommand(jobValidator, schedulerFactory);
            JobEntity result = createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

            // assert
            schedulerFactory.Received(1).GetScheduler();
            scheduler.DidNotReceive().ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>());
        }


        [Test]
        public void Execute_ValidJob_ValidTriggerCreated()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().Build();

            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);
            scheduler.When(x => x.ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>())).Do((c) =>
            {
                ITrigger trigger = c.ArgAt<ITrigger>(1);
                Assert.That(trigger.Key.Group, Is.EqualTo($"{TransferJob.GroupName}"));
                Assert.That(trigger.Key.Name, Is.EqualTo($"TransferTrigger.{jobEntity.Id}"));
            });

            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            // execute
            ICreateJobCommand createJobCommand = new CreateJobCommand(jobValidator, schedulerFactory);
            createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();
        }

        [Test]
        public void Execute_ValidJob_ValidJobCreated()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            IJobValidator jobValidator = Substitute.For<IJobValidator>();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().Build();

            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);
            scheduler.When(x => x.ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>())).Do((c) =>
            {
                IJobDetail jobDetail = c.ArgAt<IJobDetail>(0);
                Assert.That(jobDetail.Key.Group, Is.EqualTo($"{TransferJob.GroupName}"));
                Assert.That(jobDetail.Key.Name, Is.EqualTo($"TransferJob.{jobEntity.Id}"));
            });


            jobValidator.Validate(dbContext, jobEntity).Returns(new ValidationResult());

            // execute
            ICreateJobCommand createJobCommand = new CreateJobCommand(jobValidator, schedulerFactory);
            createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();
        }


        [Test]
        public void ExecuteAsync_Integration_ExecutesQueryWithoutError()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    ICreateJobCommand createJobCommand = new CreateJobCommand(new JobValidator(new HostRepository(), new DirectoryUtility()), Substitute.For<ISchedulerFactory>());
                    HostEntity host = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    DateTime dtBefore = DateTime.UtcNow;
                    JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties()
                        .WithLocalPath(AppDomain.CurrentDomain.BaseDirectory)
                        .WithHostId(host.Id)
                        .Build();

                    JobEntity result = createJobCommand.ExecuteAsync(dbContext, jobEntity).GetAwaiter().GetResult();

                    Assert.IsNotNull(result);
                    Assert.That(result.Name, Is.EqualTo(jobEntity.Name));
                    Assert.That(result.RestartOnFailure, Is.EqualTo(jobEntity.RestartOnFailure));
                    Assert.That(jobEntity.Created, Is.GreaterThanOrEqualTo(dtBefore));

                }
            }
        }


    }
}
