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
    public class GlobalUserSettingRepositoryTests
    {

        #region GetAllAsync Tests

        [Test]
        public void GlobalUserSettingAsync_OnExecute_PerformsQuery()
        {
            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContext.QueryAsync<GlobalUserSettingEntity>(Arg.Any<string>()).Returns(new[] { new GlobalUserSettingEntity(), new GlobalUserSettingEntity() });

			GlobalUserSettingRepository repo = new GlobalUserSettingRepository();
            IEnumerable<GlobalUserSettingEntity> result = repo.GetAllAsync(dbContext).Result;

            dbContext.Received(1).QueryAsync<GlobalUserSettingEntity>(Arg.Any<string>());
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

                    GlobalUserSettingEntity settingEntity1 = dbIntegrationTestHelper.CreateGlobalUserSettingEntity(dbContext, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
					GlobalUserSettingEntity settingEntity2 = dbIntegrationTestHelper.CreateGlobalUserSettingEntity(dbContext, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
					GlobalUserSettingEntity settingEntity3 = dbIntegrationTestHelper.CreateGlobalUserSettingEntity(dbContext, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

					GlobalUserSettingRepository settingRepo = new GlobalUserSettingRepository();
					GlobalUserSettingEntity[] result = settingRepo.GetAllAsync(dbContext).Result.ToArray();

                    Assert.That(result.Length, Is.EqualTo(3));
                    Assert.That(result.SingleOrDefault(x => x.Id == settingEntity1.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == settingEntity2.Id), Is.Not.Null);
                    Assert.That(result.SingleOrDefault(x => x.Id == settingEntity3.Id), Is.Not.Null);
                }
            }
        }

        #endregion


    }
}
