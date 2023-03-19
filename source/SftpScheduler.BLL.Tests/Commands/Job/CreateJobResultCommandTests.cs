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
    public class CreateJobResultCommandTests
    {


        [Test]
        public void Execute_ValidJobResult_PopulatesJobResultIdOnReturnValue()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();

            var jobEntity = EntityTestHelper.CreateJobEntity();
            int newEntityId = Faker.RandomNumber.Next();

            dbContext.ExecuteScalarAsync<int>(Arg.Any<string>()).Returns(newEntityId);

            CreateJobResultCommand createJobResultCommand = new CreateJobResultCommand();
            JobResultEntity result = createJobResultCommand.ExecuteAsync(dbContext, jobEntity.Id).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteScalarAsync<int>(Arg.Any<string>());
            Assert.That(result.Id, Is.EqualTo(newEntityId));
        }

        [Test]
        public void Execute_ValidJobResult_ValidJobResultCreated()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();

            var jobEntity = EntityTestHelper.CreateJobEntity();


            // execute
            CreateJobResultCommand createJobCommand = new CreateJobResultCommand();
            createJobCommand.ExecuteAsync(dbContext, jobEntity.Id).GetAwaiter().GetResult();

            dbContext.Received(1).ExecuteNonQueryAsync(Arg.Any<string>(), Arg.Any<JobResultEntity>());
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
                    CreateJobResultCommand createJobResultCommand = new CreateJobResultCommand();
                    HostEntity host = dbIntegrationTestHelper.CreateHostEntity(dbContext);
                    JobEntity job = dbIntegrationTestHelper.CreateJobEntity(dbContext, host.Id);

                    DateTime dtBefore = DateTime.Now;
                    JobResultEntity result = createJobResultCommand.ExecuteAsync(dbContext, job.Id).GetAwaiter().GetResult();

                    Assert.IsNotNull(result);
                    Assert.That(result.Status, Is.EqualTo(JobResult.InProgress));
                    Assert.That(result.StartDate, Is.GreaterThanOrEqualTo(dtBefore));
                    Assert.IsNull(result.EndDate);

                }
            }
        }


    }
}
