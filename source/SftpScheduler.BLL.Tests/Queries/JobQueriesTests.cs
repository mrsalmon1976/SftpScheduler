using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
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
            dbContext.QueryAsync<JobEntity>(Arg.Any<string>()).Returns(new[] { new JobEntity(), new JobEntity() });

            JobRepository jobQueries = new JobRepository();
            IEnumerable<JobEntity> result = jobQueries.GetAllAsync(dbContext).Result;

            dbContext.Received(1).QueryAsync<JobEntity>(Arg.Any<string>());
            Assert.That(result.Count(), Is.EqualTo(2));

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

                    JobEntity jobEntity1 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    JobEntity jobEntity2 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    JobEntity jobEntity3 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);

                    JobRepository jobQueries = new JobRepository();
                    JobEntity[] result = jobQueries.GetAllAsync(dbContext).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(3));
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
            JobEntity jobEntity = new JobEntity();   
            dbContext.QueryAsync<JobEntity>(Arg.Any<string>(), Arg.Any<object>()).Returns(new[] { jobEntity });
            
            JobRepository jobQueries = new JobRepository();
            jobQueries.GetByIdAsync(dbContext, jobEntity.Id).GetAwaiter().GetResult();

            dbContext.Received(1).QuerySingleAsync<JobEntity>(Arg.Any<string>(), Arg.Any<object>());

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
                    JobEntity jobEntity = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);

                    JobRepository jobQueries = new JobRepository();
                    JobEntity result = jobQueries.GetByIdAsync(dbContext, jobEntity.Id).Result;

                    Assert.That(result.Id, Is.EqualTo(jobEntity.Id));
                    Assert.That(result.Name, Is.EqualTo(jobEntity.Name));
                }
            }
        }

        #endregion

    }
}
