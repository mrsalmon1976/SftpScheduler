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
    public class CreateJobCommandTests : DbIntegrationTestBase
    {
        [Test]
        public void Execute_ValidJob_ExecutesQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            string jobName = Guid.NewGuid().ToString();

            CreateJobCommand createJobCommand = new CreateJobCommand();
            createJobCommand.Execute(dbContext, jobName);

            dbContext.Received(1).ExecuteNonQuery(Arg.Any<string>(), Arg.Any<JobEntity>());
        }

        [Test]
        public void Execute_Integration_ExecutesQueryWithoutError()
        {
            this.CreateDatabase();

            CreateJobCommand createJobCommand = new CreateJobCommand();
            using (IDbContext dbContext = this.DbContextFactory.GetDbContext())
            {
                string jobName = Guid.NewGuid().ToString();
                DateTime dtBefore = DateTime.UtcNow;

                JobEntity jobEntity = createJobCommand.Execute(dbContext, jobName);

                Assert.IsNotNull(jobEntity);
                Assert.AreEqual(jobName, jobEntity.Name);
                Assert.That(jobEntity.Created, Is.GreaterThanOrEqualTo(dtBefore));

            }
        }


    }
}
