﻿using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SftpScheduler.BLL.Tests.Repositories
{
    [TestFixture]
    public class HostRepositoryTests
    {

        #region GetAll Tests

        [Test]
        public void GetAll_OnExecute_PerformsQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContext.QueryAsync<HostEntity>(Arg.Any<string>()).Returns(new[] { new HostEntity(), new HostEntity() });

            HostRepository hostQueries = new HostRepository();
            IEnumerable<HostEntity> result = hostQueries.GetAllAsync(dbContext).Result;

            dbContext.Received(1).QueryAsync<HostEntity>(Arg.Any<string>());
            Assert.That(result.Count(), Is.EqualTo(2));

        }

        [Test]
        public void GetAll_Integration_ReturnsDbRecords()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity HostEntity1 = dbIntegrationTestHelper.CreateHostEntity(dbContext);
                    HostEntity HostEntity2 = dbIntegrationTestHelper.CreateHostEntity(dbContext);
                    HostEntity HostEntity3 = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    HostRepository hostQueries = new HostRepository();
                    HostEntity[] result = hostQueries.GetAllAsync(dbContext).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(3));
                    Assert.That(result.SingleOrDefault(x => x.Id == HostEntity1.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == HostEntity2.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == HostEntity3.Id), Is.Not.Null);
                }
            }
        }

        #endregion

        #region GetByIdAsync Tests

        [Test]
        public void GetById_OnExecute_PerformsQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            HostEntity hostEntity = new HostEntity();   
            dbContext.QueryAsync<HostEntity>(Arg.Any<string>(), Arg.Any<object>()).Returns(new[] { hostEntity });
            
            HostRepository hostQueries = new HostRepository();
            HostEntity result = hostQueries.GetByIdAsync(dbContext, hostEntity.Id).Result;

            dbContext.Received(1).QuerySingleAsync<HostEntity>(Arg.Any<string>(), Arg.Any<object>());

        }

        [Test]
        public void GetById_Integration_ReturnsDbRecord()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    HostRepository hostQueries = new HostRepository();
                    HostEntity result = hostQueries.GetByIdAsync(dbContext, hostEntity.Id).Result;

                    Assert.That(result.Id, Is.EqualTo(hostEntity.Id));
                    Assert.That(result.Name, Is.EqualTo(hostEntity.Name));
                }
            }
        }

        #endregion

        #region GetByIdOrDefaultAsync Tests

        [Test]
        public void GetByIdOrDefault_OnExecute_PerformsQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            HostEntity hostEntity = new HostEntity();
            dbContext.QueryAsync<HostEntity>(Arg.Any<string>(), Arg.Any<object>()).Returns(new[] { hostEntity });

            HostRepository hostRepo = new HostRepository();
            HostEntity result = hostRepo.GetByIdOrDefaultAsync(dbContext, hostEntity.Id).Result;

            dbContext.Received(1).QuerySingleOrDefaultAsync<HostEntity>(Arg.Any<string>(), Arg.Any<object>());

        }

        [Test]
        public void GetByIdOrDefault_Integration_ReturnsDbRecord()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    HostRepository hostRepo = new HostRepository();
                    HostEntity result = hostRepo.GetByIdOrDefaultAsync(dbContext, hostEntity.Id).Result;

                    Assert.That(result.Id, Is.EqualTo(hostEntity.Id));
                    Assert.That(result.Name, Is.EqualTo(hostEntity.Name));
                }
            }
        }

        [Test]
        public void GetByIdOrDefault_Integration_ReturnsNullWhenNoRecord()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostRepository hostRepo = new HostRepository();
                    HostEntity result = hostRepo.GetByIdOrDefaultAsync(dbContext, 100).Result;

                    Assert.That(result, Is.Null);
                }
            }
        }


        #endregion

        #region GetAllJobCountsAsync Tests

        [Test]
        public void GetAllJobCountsAsync_OnExecute_PerformsQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();

            HostRepository hostRepo = new HostRepository();
            IEnumerable<HostJobCountEntity> result = hostRepo.GetAllJobCountsAsync(dbContext).Result;

            dbContext.Received(1).QueryAsync<HostJobCountEntity>(Arg.Any<string>());

        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void GetAllJobCountsAsync_Integration_ReturnsDbRecord(int jobCount)
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    for (int i=0; i<jobCount; i++)
                    {
                        dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    }

                    HostRepository hostRepo = new HostRepository();
                    HostJobCountEntity result = hostRepo.GetAllJobCountsAsync(dbContext).Result.SingleOrDefault();

                    Assert.That(result.HostId, Is.EqualTo(hostEntity.Id));
                    Assert.That(result.JobCount, Is.EqualTo(jobCount));
                }
            }
        }

        #endregion

        #region GetJobCountAsync Tests

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void GetJobCountAsync_Integration_ReturnsCorrectCount(int jobCount)
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    HostEntity hostEntity = dbIntegrationTestHelper.CreateHostEntity(dbContext);

                    for (int i = 0; i < jobCount; i++)
                    {
                        dbIntegrationTestHelper.CreateJobEntity(dbContext, hostEntity.Id);
                    }

                    HostRepository hostRepo = new HostRepository();
                    int result = hostRepo.GetJobCountAsync(dbContext, hostEntity.Id).Result;

                    Assert.That(result, Is.EqualTo(jobCount));
                }
            }
        }

        #endregion
    }
}
