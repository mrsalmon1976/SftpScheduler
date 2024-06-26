﻿using NSubstitute;
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
    public class JobRepositoryTests
    {

        #region GetAllAsync Tests

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

        #region GetAllCountAsync Tests

        [Test]
        public void GetAllCountAsync_Integration_ReturnsDbRecords()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);
                    int count = Faker.RandomNumber.Next(3, 10);

                    for (int i= 0; i< count; i++)
                    {
                        dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    }

                    JobRepository jobRepo = new JobRepository();
                    int result = jobRepo.GetAllCountAsync(dbContext).Result;

                    Assert.That(result, Is.EqualTo(count));
                }
            }
        }

        #endregion

        #region GetAllFailingAsync Tests

        [Test]
        public void GetAllFailingAsync_IntegrationNoRecords_ReturnsEmptyEnum()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    JobRepository jobRepo = new JobRepository();
                    JobEntity[] result = jobRepo.GetAllFailingAsync(dbContext).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(0));
                }
            }
        }

        [Test]
        public void GetAllFailingAsync_IntegrationWithEnabledAndDisabledJobs_ReturnsAllJobs()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    JobEntity jobEntity1 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id, isEnabled: true);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity1.Id, JobStatus.Failed);

                    JobEntity jobEntity2 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id, isEnabled: true);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity2.Id, JobStatus.Success);

                    JobEntity jobEntity3 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id, isEnabled: false);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity3.Id, JobStatus.Failed);

                    JobEntity jobEntity4 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id, isEnabled: false);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity4.Id, JobStatus.Success);

                    JobRepository jobRepo = new JobRepository();
                    JobEntity[] result = jobRepo.GetAllFailingAsync(dbContext).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(2));
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity1.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity2.Id), Is.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity3.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity4.Id), Is.Null);
                }
            }
        }

        #endregion

        #region GetAllFailingActiveAsync Tests

        [Test]
        public void GetAllFailingActiveAsync_IntegrationNoRecords_ReturnsEmptyEnum()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    JobRepository jobRepo = new JobRepository();
                    JobEntity[] result = jobRepo.GetAllFailingActiveAsync(dbContext).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(0));
                }
            }
        }

        [Test]
        public void GetAllFailingActiveAsync_IntegrationOnlyFailingJobs_ReturnsAllJobs()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    JobEntity jobEntity1 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity1.Id, JobStatus.Success);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity1.Id, JobStatus.Success);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity1.Id, JobStatus.Failed);

                    JobEntity jobEntity2 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity2.Id, JobStatus.Failed);

                    JobRepository jobRepo = new JobRepository();
                    JobEntity[] result = jobRepo.GetAllFailingActiveAsync(dbContext).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(2));
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity1.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity2.Id), Is.Not.Null);
                }
            }
        }

        [Test]
        public void GetAllFailingActiveAsync_IntegrationSomeFailingJobs_ReturnsJobs()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    JobEntity jobEntity1 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity1.Id, JobStatus.Success);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity1.Id, JobStatus.Success);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity1.Id, JobStatus.Failed);

                    JobEntity jobEntity2 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity2.Id, JobStatus.Failed);

                    JobEntity jobEntity3 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity3.Id, JobStatus.Success);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity3.Id, JobStatus.Success);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity3.Id, JobStatus.Success);

                    JobRepository jobRepo = new JobRepository();
                    JobEntity[] result = jobRepo.GetAllFailingActiveAsync(dbContext).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(2));
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity1.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity2.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity3.Id), Is.Null);
                }
            }
        }

        [Test]
        public void GetAllFailingActiveAsync_IntegrationWithDisabledJobs_OnlyReturnsEnabledJobs()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    JobEntity jobEntity1 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity1.Id, JobStatus.Failed);

                    JobEntity jobEntity2 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id, false);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity2.Id, JobStatus.Failed);

                    JobEntity jobEntity3 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity3.Id, JobStatus.Failed);

                    JobRepository jobRepo = new JobRepository();
                    JobEntity[] result = jobRepo.GetAllFailingActiveAsync(dbContext).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(2));
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity1.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity2.Id), Is.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity3.Id), Is.Not.Null);
                }
            }
        }

        #endregion

        #region GetAllFailedSinceAsync Tests

        [Test]
        public void GetAllFailedSinceAsync_IntegrationNoRecords_ReturnsEmptyEnum()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    JobRepository jobRepo = new JobRepository();
                    JobEntity[] result = jobRepo.GetAllFailedSinceAsync(dbContext, DateTime.Now).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(0));
                }
            }
        }

        [Test]
        public void GetAllFailedSinceAsync_IntegrationOnlyFailuresBeforeOffset_ReturnsAllJobs()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    JobEntity jobEntity1 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity1.Id, JobStatus.Failed);

                    JobEntity jobEntity2 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity2.Id, JobStatus.Failed);

                    JobRepository jobRepo = new JobRepository();
                    JobEntity[] result = jobRepo.GetAllFailedSinceAsync(dbContext, DateTime.Now.AddSeconds(1)).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(0));
                }
            }
        }

        [Test]
        public void GetAllFailedSinceAsync_IntegrationSomeFailedJobs_ReturnsJobs()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    JobEntity jobEntity1 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity1.Id, JobStatus.Success);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity1.Id, JobStatus.Success);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity1.Id, JobStatus.Failed);

                    JobEntity jobEntity2 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity2.Id, JobStatus.Failed);

                    JobEntity jobEntity3 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity3.Id, JobStatus.Success);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity3.Id, JobStatus.Success);
                    dbIntegrationTestHelper.CreateJobLogEntity(dbContext, jobEntity3.Id, JobStatus.Success);

                    JobRepository jobRepo = new JobRepository();
                    JobEntity[] result = jobRepo.GetAllFailedSinceAsync(dbContext, DateTime.Now.AddMinutes(-10)).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(2));
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity1.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity2.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == jobEntity3.Id), Is.Null);
                }
            }
        }

        #endregion

        #region GetAllFailedSinceAsync Tests

        [Test]
        public void GetAllWithoutTransfersBetweenAsync_IntegrationNoRecords_ReturnsEmptyEnum()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    JobRepository jobRepo = new JobRepository();
                    JobEntity[] result = jobRepo.GetAllWithoutTransfersBetweenAsync(dbContext, DateTime.Now, DateTime.Now.AddDays(1)).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(0));
                }
            }
        }

        [Test]
        public void GetAllWithoutTransfersBetweenAsync_IntegrationSomeJobsWithoutTransfers_ReturnsOnlyThoseWithoutTransfers()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    JobEntity jobEntity1 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    JobEntity jobEntity2 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);

                    DateTime startDate = DateTime.Now.AddDays(-3);
                    DateTime endDate = DateTime.Now.AddDays(-1);

                    JobFileLogEntity jobFileLogEntityInRange1 = dbIntegrationTestHelper.CreateJobFileLogEntity(dbContext, jobEntity1.Id, startDate.AddMilliseconds(1));
                    JobFileLogEntity jobFileLogEntityInRange2 = dbIntegrationTestHelper.CreateJobFileLogEntity(dbContext, jobEntity2.Id, endDate.AddMilliseconds(-1));

                    JobFileLogEntity jobFileLogEntityOutOfRange1 = dbIntegrationTestHelper.CreateJobFileLogEntity(dbContext, jobEntity1.Id, startDate.AddMilliseconds(-1));
                    JobFileLogEntity jobFileLogEntityOutOfRange2 = dbIntegrationTestHelper.CreateJobFileLogEntity(dbContext, jobEntity2.Id, endDate.AddMilliseconds(1));

                    JobRepository jobRepo = new JobRepository();
                    JobEntity[] result = jobRepo.GetAllWithoutTransfersBetweenAsync(dbContext, startDate, endDate).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(2));
                    Assert.That(result.FirstOrDefault(x => x.Id == jobFileLogEntityInRange1.Id), Is.Not.Null);
                    Assert.That(result.FirstOrDefault(x => x.Id == jobFileLogEntityInRange2.Id), Is.Not.Null);
                }
            }
        }

        [Test]
        public void GetAllWithoutTransfersBetweenAsync_IntegrationAllJobsWithTransfers_ReturnsEmpty()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    JobEntity jobEntity1 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    JobEntity jobEntity2 = dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);

                    DateTime startDate = DateTime.Now.AddDays(-3);
                    DateTime endDate = DateTime.Now;

                    dbIntegrationTestHelper.CreateJobFileLogEntity(dbContext, jobEntity1.Id, startDate.AddMilliseconds(1));
                    dbIntegrationTestHelper.CreateJobFileLogEntity(dbContext, jobEntity1.Id, startDate.AddMilliseconds(2));
                    dbIntegrationTestHelper.CreateJobFileLogEntity(dbContext, jobEntity2.Id, startDate.AddMilliseconds(3));


                    JobRepository jobRepo = new JobRepository();
                    JobEntity[] result = jobRepo.GetAllWithoutTransfersBetweenAsync(dbContext, startDate, endDate).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(0));
                }
            }
        }

        [Test]
        public void GetAllWithoutTransfersBetweenAsync_IntegrationAllJobsWithoutTransfers_ReturnsAllJobs()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);

                    DateTime startDate = DateTime.Now.AddDays(-3);
                    DateTime endDate = DateTime.Now;

                    JobRepository jobRepo = new JobRepository();
                    JobEntity[] result = jobRepo.GetAllWithoutTransfersBetweenAsync(dbContext, startDate, endDate).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(2));
                }
            }
        }

        #endregion
        #region GetByIdAsync Tests

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
