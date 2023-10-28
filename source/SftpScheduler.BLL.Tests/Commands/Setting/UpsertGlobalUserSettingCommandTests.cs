using NUnit.Framework;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Commands.Setting;

namespace SftpScheduler.BLL.Tests.Commands.Setting
{
    [TestFixture]
    public class UpsertGlobalUserSettingCommandTests
	{

        [Test]
        public void ExecuteAsync_Integration_SettingDoesNotExist_NewRecordInserted()
        {
            using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
            {
                dbIntegrationTestHelper.CreateDatabase();

                using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
                {
                    string settingId = Guid.NewGuid().ToString();
					string settingValue = Guid.NewGuid().ToString();

                    // make sure the setting does not exist
                    GlobalUserSettingRepository repo = new GlobalUserSettingRepository();
                    IEnumerable<GlobalUserSettingEntity> settings = repo.GetAllAsync(dbContext).GetAwaiter().GetResult();
                    Assert.That(settings.SingleOrDefault(x => x.Id == settingId), Is.Null);

                    // do the upsert
                    GlobalUserSettingEntity settingEntity = new GlobalUserSettingEntity(settingId, settingValue);

					IUpsertGlobalUserSettingCommand cmd = new UpsertGlobalUserSettingCommand();
                    cmd.Execute(dbContext, settingEntity).GetAwaiter().GetResult();

					// load the entity
					IEnumerable<GlobalUserSettingEntity> settingsAfterInsert = repo.GetAllAsync(dbContext).GetAwaiter().GetResult();
					GlobalUserSettingEntity? result = settingsAfterInsert.SingleOrDefault(x => x.Id == settingId);
					Assert.That(result, Is.Not.Null);
					Assert.That(result.SettingValue, Is.EqualTo(settingValue));
                }
            }
        }

		[Test]
		public void ExecuteAsync_Integration_SettingExists_ExistingRecordUpdated()
		{
			using (DbIntegrationTestHelper dbIntegrationTestHelper = new DbIntegrationTestHelper())
			{
				dbIntegrationTestHelper.CreateDatabase();

				using (IDbContext dbContext = dbIntegrationTestHelper.DbContextFactory.GetDbContext())
				{
					string settingId = Guid.NewGuid().ToString();
					string settingValue = Guid.NewGuid().ToString();

					GlobalUserSettingEntity settingEntity = dbIntegrationTestHelper.CreateGlobalUserSettingEntity(dbContext, settingId, Guid.NewGuid().ToString());

					// make sure the setting exists
					GlobalUserSettingRepository repo = new GlobalUserSettingRepository();
					IEnumerable<GlobalUserSettingEntity> settings = repo.GetAllAsync(dbContext).GetAwaiter().GetResult();
					Assert.That(settings.SingleOrDefault(x => x.Id == settingId), Is.Not.Null);

					// do the upsert
					GlobalUserSettingEntity updatedEntity = new GlobalUserSettingEntity(settingId, settingValue);
					IUpsertGlobalUserSettingCommand cmd = new UpsertGlobalUserSettingCommand();
					cmd.Execute(dbContext, updatedEntity).GetAwaiter().GetResult();

					// load the entity
					IEnumerable<GlobalUserSettingEntity> settingsAfterUpsert = repo.GetAllAsync(dbContext).GetAwaiter().GetResult();
					GlobalUserSettingEntity? result = settingsAfterUpsert.SingleOrDefault(x => x.Id == settingId);
					Assert.That(result, Is.Not.Null);
					Assert.That(result.SettingValue, Is.EqualTo(settingValue));
				}
			}
		}
	}
	
}
