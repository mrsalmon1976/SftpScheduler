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
    public class JobFileLogRepositoryTests
    {

        #region GetAllByJobAsync Tests

        [Test]
        public void GetAllByJobAsync_OnExecute_PerformsQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContext.QueryAsync<JobFileLogEntity>(Arg.Any<string>(), Arg.Any<object>()).Returns(new[] { new JobFileLogEntity(), new JobFileLogEntity() });

            JobFileLogRepository jobLogRepo = new JobFileLogRepository();
            IEnumerable<JobFileLogEntity> result = jobLogRepo.GetAllByJobAsync(dbContext, 1, 2, 3).Result;

            dbContext.Received(1).QueryAsync<JobFileLogEntity>(Arg.Any<string>(), Arg.Any<object>());
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

                    List<JobFileLogEntity> jobLogs = CreateJobFileLogs(dbIntegrationTestHelper, dbContext, jobEntity.Id, 3);

                    JobFileLogRepository jobLogRepo = new JobFileLogRepository();
                    JobFileLogEntity[] result = jobLogRepo.GetAllByJobAsync(dbContext, jobEntity.Id, Int32.MaxValue, 100).Result.ToArray();

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

                    List<JobFileLogEntity> jobLogs = CreateJobFileLogs(dbIntegrationTestHelper, dbContext, jobEntity.Id, 4);

                    JobFileLogRepository jobLogRepo = new JobFileLogRepository();
                    JobFileLogEntity[] result = jobLogRepo.GetAllByJobAsync(dbContext, jobEntity.Id, Int32.MaxValue, 2).Result.ToArray();

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

                    List<JobFileLogEntity> jobLogs = CreateJobFileLogs(dbIntegrationTestHelper, dbContext, jobEntity.Id, 4);

                    JobFileLogRepository jobLogRepo = new JobFileLogRepository();
                    JobFileLogEntity[] result = jobLogRepo.GetAllByJobAsync(dbContext, jobEntity.Id, jobLogs[2].Id, 100).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(3));
                    Assert.That(result.SingleOrDefault(x => x.Id == jobLogs[3].Id), Is.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobLogs[2].Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobLogs[1].Id), Is.Not.Null);
                }
            }
        }

        #endregion

        private List<JobFileLogEntity> CreateJobFileLogs(DbIntegrationTestHelper dbIntegrationTestHelper, IDbContext dbContext, int jobId, int count)
        {
            List<JobFileLogEntity> jobLogs = new List<JobFileLogEntity>();
            for (int i=0; i< count; i++)
            {
                JobFileLogEntity jobLogEntity = dbIntegrationTestHelper.CreateJobFileLogEntity(dbContext, jobId);
                jobLogs.Add(jobLogEntity);
            }
            return jobLogs;
        }

        

    }
}
