using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.Queries
{
    [TestFixture]
    public class JobQueriesTests : DbIntegrationTestBase
    {
        [Test]
        public void GetById_OnExecute_PerformsQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            JobEntity jobEntity = new JobEntity();   
            dbContext.Query<JobEntity>(Arg.Any<string>(), Arg.Any<object>()).Returns(new[] { jobEntity });
            
            JobQueries jobQueries = new JobQueries();
            JobEntity result = jobQueries.GetById(dbContext, jobEntity.Id);

            dbContext.Received(1).Query<JobEntity>(Arg.Any<string>(), Arg.Any<object>());

        }

        [Test]
        public void GetById_Integration_ReturnsDbRecord()
        {
            this.CreateDatabase();

            using (IDbContext dbContext = this.DbContextFactory.GetDbContext())
            {
                JobEntity jobEntity = this.CreateJobEntity(dbContext);

                JobQueries jobQueries = new JobQueries();
                JobEntity result = jobQueries.GetById(dbContext, jobEntity.Id);

                Assert.AreEqual(result.Id, jobEntity.Id);
                Assert.AreEqual(result.Name, jobEntity.Name);
            }
        }

    }
}
