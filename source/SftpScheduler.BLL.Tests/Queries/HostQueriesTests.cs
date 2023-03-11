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
    public class HostQueriesTests
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
            Assert.AreEqual(2, result.Count());

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

                    Assert.That(3, Is.EqualTo(result.Length));
                    Assert.That(result.SingleOrDefault(x => x.Id == HostEntity1.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == HostEntity2.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == HostEntity3.Id), Is.Not.Null);
                }
            }
        }

        #endregion

        #region GetById Tests

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

                    Assert.AreEqual(result.Id, hostEntity.Id);
                    Assert.AreEqual(result.Name, hostEntity.Name);
                }
            }
        }

        #endregion

    }
}
