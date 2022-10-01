using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Command.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.Commands.Job
{
    [TestFixture]
    public class CreateJobCommandTests
    {
        [Test]
        public void Execute_ValidJob_ExecutesQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            string jobName = Faker.Lorem.GetFirstWord();
            int hostId = Faker.RandomNumber.Next();


            CreateJobCommand createJobCommand = new CreateJobCommand();
            createJobCommand.Execute(dbContext, jobName, hostId);

            dbContext.Received(1).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<JobEntity>());
        }

        [Test]
        public void Execute_Integration_ExecutesQueryWithoutError()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                CreateJobCommand createJobCommand = new CreateJobCommand();
                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity host = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    string jobName = Guid.NewGuid().ToString();
                    DateTime dtBefore = DateTime.UtcNow;

                    JobEntity jobEntity = createJobCommand.Execute(dbContext, jobName, host.Id);

                    Assert.IsNotNull(jobEntity);
                    Assert.AreEqual(jobName, jobEntity.Name);
                    Assert.That(jobEntity.Created, Is.GreaterThanOrEqualTo(dtBefore));

                }
            }
        }


    }
}
