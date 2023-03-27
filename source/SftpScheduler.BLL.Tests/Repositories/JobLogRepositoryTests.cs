using Microsoft.EntityFrameworkCore;
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

namespace SftpScheduler.BLL.Tests.Repositories
{
    [TestFixture]
    public class JobLogRepositoryTests
    {

        #region GetAllByJobAsync Tests

        [Test]
        public void GetAllByJobAsync_OnExecute_PerformsQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContext.QueryAsync<JobLogEntity>(Arg.Any<string>(), Arg.Any<object>()).Returns(new[] { new JobLogEntity(), new JobLogEntity() });

            JobLogRepository jobLogRepo = new JobLogRepository();
            IEnumerable<JobLogEntity> result = jobLogRepo.GetAllByJobAsync(dbContext, 1, 2, 3).Result;

            dbContext.Received(1).QueryAsync<JobLogEntity>(Arg.Any<string>(), Arg.Any<object>());
            Assert.That(result.Count(), Is.EqualTo(2));

        }

        [Test]
        public void GetAllByJobAsync_Integration_ReturnsDbRecords()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    JobEntity jobEntity = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);

                    List<JobLogEntity> jobLogs = CreateJobLogs(dbIntegrationTestHelper, dbContext, jobEntity.Id, 3);

                    JobLogRepository jobLogRepo = new JobLogRepository();
                    JobLogEntity[] result = jobLogRepo.GetAllByJobAsync(dbContext, jobEntity.Id, Int32.MaxValue, 100).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(3));
                    Assert.That(result.SingleOrDefault(x => x.Id == jobLogs[0].Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobLogs[1].Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobLogs[2].Id), Is.Not.Null);
                }
            }
        }

        [Test]
        public void GetAllByJobAsync_Integration_ReturnsLimit()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    JobEntity jobEntity = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);

                    List<JobLogEntity> jobLogs = CreateJobLogs(dbIntegrationTestHelper, dbContext, jobEntity.Id, 4);

                    JobLogRepository jobLogRepo = new JobLogRepository();
                    JobLogEntity[] result = jobLogRepo.GetAllByJobAsync(dbContext, jobEntity.Id, Int32.MaxValue, 2).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(2));
                    Assert.That(result.SingleOrDefault(x => x.Id == jobLogs[3].Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobLogs[2].Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobLogs[1].Id), Is.Null);
                }
            }
        }

        [Test]
        public void GetAllByJobAsync_Integration_ExcludesByMaxId()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    JobEntity jobEntity = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);

                    List<JobLogEntity> jobLogs = CreateJobLogs(dbIntegrationTestHelper, dbContext, jobEntity.Id, 4);

                    JobLogRepository jobLogRepo = new JobLogRepository();
                    JobLogEntity[] result = jobLogRepo.GetAllByJobAsync(dbContext, jobEntity.Id, jobLogs[2].Id, 100).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(3));
                    Assert.That(result.SingleOrDefault(x => x.Id == jobLogs[3].Id), Is.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobLogs[2].Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobLogs[1].Id), Is.Not.Null);
                }
            }
        }

        #endregion

        private List<JobLogEntity> CreateJobLogs(DbIntegrationTestHelper dbIntegrationTestHelper, IDbContext dbContext, int jobId, int count)
        {
            List<JobLogEntity> jobLogs = new List<JobLogEntity>();
            for (int i=0; i< count; i++)
            {
                JobLogEntity jobLogEntity = dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobId);
                jobLogs.Add(jobLogEntity);
            }
            return jobLogs;
        }

        

    }
}
