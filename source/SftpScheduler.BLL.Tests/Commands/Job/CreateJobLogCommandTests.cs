using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.Test.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace SftpScheduler.BLL.Tests.Commands.Job
{
    [TestFixture]
    public class CreateJobLogCommandTests
    {


        [Test]
        public void Execute_ValidJobLog_PopulatesJobLogIdOnReturnValue()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();

            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().Build();
            int newEntityId = Faker.RandomNumber.Next();

            dbContext.ExecuteScalarAsync<int>(Arg.Any<string>()).Returns(newEntityId);

            CreateJobLogCommand createJobLogCommand = new CreateJobLogCommand();
            JobLogEntity result = createJobLogCommand.ExecuteAsync(dbContext, jobEntity.Id).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());
            Assert.That(result.Id, Is.EqualTo(newEntityId));
        }

        [Test]
        public void Execute_ValidJobLog_ValidJobLogCreated()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();

            JobEntity jobEntity = new SubstituteBuilder<JobEntity>().WithRandomProperties().Build();


            // execute
            CreateJobLogCommand createJobCommand = new CreateJobLogCommand();
            createJobCommand.ExecuteAsync(dbContext, jobEntity.Id).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), Arg.Any<JobLogEntity>());
            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());

        }


        [Test]
        public void ExecuteAsync_Integration_ExecutesQueryWithoutError()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    CreateJobLogCommand createJobLogCmd = new CreateJobLogCommand();
                    HostEntity host = dbIntegrationTestHelper.CreateHostEntity(dbContext);
                    JobEntity job = dbIntegrationTestHelper.CreateJobEntity(dbContext, host.Id);

                    DateTime dtBefore = DateTime.Now;
                    JobLogEntity result = createJobLogCmd.ExecuteAsync(dbContext, job.Id).GetAwaiter().GetResult();

                    Assert.IsNotNull(result);
                    Assert.That(result.Status, Is.EqualTo(JobStatus.InProgress));
                    Assert.That(result.StartDate, Is.GreaterThanOrEqualTo(dtBefore));
                    Assert.IsNull(result.EndDate);

                }
            }
        }


    }
}
