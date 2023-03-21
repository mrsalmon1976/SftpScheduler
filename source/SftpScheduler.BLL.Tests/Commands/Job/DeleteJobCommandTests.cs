using NSubstitute;
using NUnit.Framework;
using Quartz;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
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
using SystemWrapper.IO;

namespace SftpScheduler.BLL.Tests.Commands.Job
{
    [TestFixture]
    public class DeleteJobCommandTests
    {
        [Test]
        public void Execute_OnDelete_RunsMultipleDeletes()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            int jobId = Faker.RandomNumber.Next(5, 1000);

            DeleteJobCommand deleteJobCommand = new DeleteJobCommand(Substitute.For<ISchedulerFactory>());
            deleteJobCommand.ExecuteAsync(dbContext, jobId).GetAwaiter().GetResult();

            dbContext.Received(2).ExecuteNonQueryAsync(Arg.Any<string>(), Arg.Any<object>());
        }

        [TestCase("Job")]
        [TestCase("JobResult")]
        public void Execute_OnDelete_RunsDeleteOnTable(string tableName)
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            int jobId = Faker.RandomNumber.Next(5, 1000);
            bool deleteFound = false;

            dbContext.When(x => x.ExecuteNonQueryAsync(Arg.Any<string>(), Arg.Any<object>())).Do((ci) =>
            {
                string executedSql = ci.ArgAt<string>(0);
                if (executedSql.Contains(tableName))
                {
                    deleteFound = true;
                }
            });

            DeleteJobCommand deleteJobCommand = new DeleteJobCommand(Substitute.For<ISchedulerFactory>());
            deleteJobCommand.ExecuteAsync(dbContext, jobId).GetAwaiter().GetResult();

            Assert.That(deleteFound, $"No delete on table {tableName} occurred");
        }

        [Test]
        public void Execute_OnDelete_RemovesJobTrigger()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            int jobId = Faker.RandomNumber.Next(5, 1000);
            string triggerName = TransferJob.GetTriggerKeyName(jobId);

            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();
            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);

            DeleteJobCommand deleteJobCommand = new DeleteJobCommand(schedulerFactory);
            deleteJobCommand.ExecuteAsync(dbContext, jobId).GetAwaiter().GetResult();

            scheduler.Received(1).UnscheduleJob(new TriggerKey(triggerName, TransferJob.DefaultGroup));
        }

        [Test]
        public void ExecuteAsync_Integration_DeletesFromAllTables()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    DeleteJobCommand deleteJobCommand = new DeleteJobCommand(Substitute.For<ISchedulerFactory>());
                    HostEntity host = dbIntegrationTestHelper.CreateHostEntity(dbContext);
                    JobEntity job = dbIntegrationTestHelper.CreateJobEntity(dbContext, host.Id);

                    int jobId = job.Id;
                    JobResultEntity jobResult1 = dbIntegrationTestHelper.CreateJobResultEntity(dbContext, jobId);
                    JobResultEntity jobResult2 = dbIntegrationTestHelper.CreateJobResultEntity(dbContext, jobId);

                    deleteJobCommand.ExecuteAsync(dbContext, jobId).GetAwaiter().GetResult();

                    JobRepository jobRepo = new JobRepository();
                    JobEntity jobAfterDelete = jobRepo.GetByIdOrDefaultAsync(dbContext, jobId).GetAwaiter().GetResult();
                    Assert.IsNull(jobAfterDelete);


                }
            }
        }


    }
}
