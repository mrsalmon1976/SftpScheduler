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
    public class JobQueriesTests
    {

        #region GetAll Tests

        [Test]
        public void GetAllAsync_OnExecute_PerformsQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContext.QueryAsync<JobViewModel>(Arg.Any<string>()).Returns(new[] { new JobViewModel(), new JobViewModel() });

            JobRepository jobQueries = new JobRepository();
            IEnumerable<JobViewModel> result = jobQueries.GetAllAsync(dbContext).Result;

            dbContext.Received(1).QueryAsync<JobViewModel>(Arg.Any<string>());
            Assert.AreEqual(2, result.Count());

        }

        [Test]
        public void GetAllAsync_Integration_ReturnsDbRecords()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    JobViewModel jobEntity1 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    JobViewModel jobEntity2 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    JobViewModel jobEntity3 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);

                    JobRepository jobQueries = new JobRepository();
                    JobViewModel[] result = jobQueries.GetAllAsync(dbContext).Result.ToArray();

                    Assert.That(3, Is.EqualTo(result.Length));
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity1.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity2.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity3.Id), Is.Not.Null);
                }
            }
        }

        #endregion

        #region GetById Tests

        [Test]
        public void GetByIdAsync_OnExecute_PerformsQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            JobViewModel jobEntity = new JobViewModel();   
            dbContext.QueryAsync<JobViewModel>(Arg.Any<string>(), Arg.Any<object>()).Returns(new[] { jobEntity });
            
            JobRepository jobQueries = new JobRepository();
            JobViewModel result = jobQueries.GetByIdAsync(dbContext, jobEntity.Id).Result;

            dbContext.Received(1).QuerySingleAsync<JobViewModel>(Arg.Any<string>(), Arg.Any<object>());

        }

        [Test]
        public void GetByIdAsync_Integration_ReturnsDbRecord()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);
                    JobViewModel jobEntity = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);

                    JobRepository jobQueries = new JobRepository();
                    JobViewModel result = jobQueries.GetByIdAsync(dbContext, jobEntity.Id).Result;

                    Assert.AreEqual(result.Id, jobEntity.Id);
                    Assert.AreEqual(result.Name, jobEntity.Name);
                }
            }
        }

        #endregion

    }
}
