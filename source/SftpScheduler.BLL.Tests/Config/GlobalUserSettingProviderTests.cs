using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Setting;
using SftpScheduler.BLL.Config;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Security;
using SftpScheduler.BLL.Tests.Builders.Data;
using SftpScheduler.BLL.Tests.Builders.Repositories;
using SftpScheduler.BLL.Tests.Builders.Security;
using SftpScheduler.Test.Common;
using static Dapper.SqlMapper;

namespace SftpScheduler.BLL.Tests.Config
{
	[TestFixture]
	public class GlobalUserSettingProviderTests
	{
		#region DigestDays 

		[Test]
		public void DigestDays_SettingDoesNotExist_ReturnsDefault()
		{
			// setup
			IEnumerable<GlobalUserSettingEntity> settingEntities = Enumerable.Empty<GlobalUserSettingEntity>();

			IDbContext dbContext = new DbContextBuilder().Build();
			GlobalUserSettingRepository settingRepo = new GlobalUserSettingRepositoryBuilder().WithGetAllAsyncReturns(dbContext, settingEntities).Build();

			// execute
			IGlobalUserSettingProvider provider = CreateProvider(dbContext: dbContext, globalUserSettingRepo: settingRepo);
			DayOfWeek[] result = provider.DigestDays;

			// assert
			Assert.That(result, Is.EqualTo(GlobalUserSettingProvider.DefaultDigestDays));
		}

		[TestCase("Monday")]
		[TestCase("Tuesday,Monday")]
		[TestCase("Sunday,Monday,Wednesday,Friday")]
		public void DigestDays_SettingExists_ReturnsStoredValue(string days)
		{
			// setup
			string jsonDays = JsonConvert.SerializeObject(days.Split(","));
			DayOfWeek[] daysOfWeek = days.Split(",").Select(x => Enum.Parse<DayOfWeek>(x)).ToArray();

			List<GlobalUserSettingEntity> settingEntities = new List<GlobalUserSettingEntity>() {
				new GlobalUserSettingEntity(GlobalUserSettingKey.DigestDays, jsonDays)
			};

			IDbContext dbContext = new DbContextBuilder().Build();
			GlobalUserSettingRepository settingRepo = new GlobalUserSettingRepositoryBuilder().WithGetAllAsyncReturns(dbContext, settingEntities).Build();

			// execute
			IGlobalUserSettingProvider provider = CreateProvider(dbContext: dbContext, globalUserSettingRepo: settingRepo);
			DayOfWeek[] result = provider.DigestDays;

			// assert
			Assert.That(result, Is.EqualTo(daysOfWeek));
		}

		#endregion

		#region DigestTime 

		[Test]
		public void DigestTime_SettingDoesNotExist_ReturnsDefault()
		{
			// setup
			IEnumerable<GlobalUserSettingEntity> settingEntities = Enumerable.Empty<GlobalUserSettingEntity>();

			IDbContext dbContext = new DbContextBuilder().Build();
			GlobalUserSettingRepository settingRepo = new GlobalUserSettingRepositoryBuilder().WithGetAllAsyncReturns(dbContext, settingEntities).Build();

			// execute
			IGlobalUserSettingProvider provider = CreateProvider(dbContext: dbContext, globalUserSettingRepo: settingRepo);
			int result = provider.DigestTime;

			// assert
			Assert.That(result, Is.EqualTo(GlobalUserSettingProvider.DefaultDigestTime));
		}

		[TestCase(3)]
		[TestCase(12)]
		[TestCase(23)]
		public void DigestTime_SettingExists_ReturnsStoredValue(int digestTime)
		{
			// setup
			List<GlobalUserSettingEntity> settingEntities = new List<GlobalUserSettingEntity>() {
				new GlobalUserSettingEntity(GlobalUserSettingKey.DigestTime, digestTime.ToString())
			};

			IDbContext dbContext = new DbContextBuilder().Build();
			GlobalUserSettingRepository settingRepo = new GlobalUserSettingRepositoryBuilder().WithGetAllAsyncReturns(dbContext, settingEntities).Build();

			// execute
			IGlobalUserSettingProvider provider = CreateProvider(dbContext: dbContext, globalUserSettingRepo: settingRepo);
			int result = provider.DigestTime;

			// assert
			Assert.That(result, Is.EqualTo(digestTime));
		}

		#endregion

		#region SmtpPort 

		[Test]
		public void SmtpPort_SettingDoesNotExist_ReturnsDefault()
		{
			// setup
			IEnumerable<GlobalUserSettingEntity> settingEntities = Enumerable.Empty<GlobalUserSettingEntity>();

			IDbContext dbContext = new DbContextBuilder().Build();
			GlobalUserSettingRepository settingRepo = new GlobalUserSettingRepositoryBuilder().WithGetAllAsyncReturns(dbContext, settingEntities).Build();

			// execute
			IGlobalUserSettingProvider provider = CreateProvider(dbContext: dbContext, globalUserSettingRepo: settingRepo);
			int result = provider.SmtpPort;

			// assert
			Assert.That(result, Is.EqualTo(GlobalUserSettingProvider.DefaultSmtpPort));
		}

		[TestCase(1)]
		[TestCase(8080)]
		[TestCase(65000)]
		public void SmtpPort_SettingExists_ReturnsStoredValue(int port)
		{
			// setup
			List<GlobalUserSettingEntity> settingEntities = new List<GlobalUserSettingEntity>() {
				new GlobalUserSettingEntity(GlobalUserSettingKey.SmtpPort, port.ToString())
			};

			IDbContext dbContext = new DbContextBuilder().Build();
			GlobalUserSettingRepository settingRepo = new GlobalUserSettingRepositoryBuilder().WithGetAllAsyncReturns(dbContext, settingEntities).Build();

			// execute
			IGlobalUserSettingProvider provider = CreateProvider(dbContext: dbContext, globalUserSettingRepo: settingRepo);
			int result = provider.SmtpPort;

			// assert
			Assert.That(result, Is.EqualTo(port));
		}

		#endregion

		#region UpdateChangedValue

		[TestCase(GlobalUserSettingKey.SmtpUserName)]
		[TestCase(GlobalUserSettingKey.SmtpFromEmail)]
		public void UpdateChangedValue_StringsUnchanged_UpdateCommandNotExecuted(string settingKey)
		{
			// setup 
			IDbContext dbContext = new DbContextBuilder().Build();
			IUpsertGlobalUserSettingCommand upsertGlobalUserSettingCommand = new SubstituteBuilder<IUpsertGlobalUserSettingCommand>().Build();

			// execute
			IGlobalUserSettingProvider provider = CreateProvider(upsertGlobalUserSettingCommand: upsertGlobalUserSettingCommand);
			bool result = provider.UpdateChangedValue(dbContext, settingKey, "test", "test").Result;

			// assert
			Assert.That(result, Is.False);
			upsertGlobalUserSettingCommand.DidNotReceive().Execute(Arg.Any<IDbContext>(), Arg.Any<GlobalUserSettingEntity>());
		}

		[TestCase(GlobalUserSettingKey.SmtpUserName)]
		[TestCase(GlobalUserSettingKey.SmtpFromEmail)]
		public void UpdateChangedValue_StringsChanged_UpdateCommandExecuted(string settingKey)
		{
			// setup 
			string newValue = Guid.NewGuid().ToString();
			IDbContext dbContext = new DbContextBuilder().Build();
            IUpsertGlobalUserSettingCommand upsertGlobalUserSettingCommand = new SubstituteBuilder<IUpsertGlobalUserSettingCommand>().Build();

            // execute
            IGlobalUserSettingProvider provider = CreateProvider(dbContext: dbContext, upsertGlobalUserSettingCommand: upsertGlobalUserSettingCommand);
			bool result = provider.UpdateChangedValue(dbContext, settingKey, "old", newValue).Result;

			// assert
			Assert.That(result, Is.True);
			upsertGlobalUserSettingCommand.Received(1).Execute(dbContext, new GlobalUserSettingEntity(settingKey, newValue));
		}

		[TestCase(GlobalUserSettingKey.SmtpPassword)]
		public void UpdateChangedValue_StringKeyIsPasswordKey_ValueEncryptedBeforeUpdate(string settingKey)
		{
			// setup 
			string newValue = Guid.NewGuid().ToString();
			string newEncryptedValue = Guid.NewGuid().ToString();
			IDbContext dbContext = new DbContextBuilder().Build();
			IEncryptionProvider encryptionProvider = new EncryptionProviderBuilder().WithEncryptReturns(newValue, newEncryptedValue).Build();
            IUpsertGlobalUserSettingCommand upsertGlobalUserSettingCommand = new SubstituteBuilder<IUpsertGlobalUserSettingCommand>().Build();

            // execute
            IGlobalUserSettingProvider provider = CreateProvider(dbContext: dbContext, encryptionProvider: encryptionProvider, upsertGlobalUserSettingCommand: upsertGlobalUserSettingCommand);
			bool result = provider.UpdateChangedValue(dbContext, settingKey, "old", newValue).Result;

			// assert
			Assert.That(result, Is.True);
			encryptionProvider.Received(1).Encrypt(newValue);
			upsertGlobalUserSettingCommand.Received(1).Execute(dbContext, new GlobalUserSettingEntity(settingKey, newEncryptedValue));
		}

		[TestCase(GlobalUserSettingKey.DigestDays, "1,2,3", "1,2,3")]
		[TestCase(GlobalUserSettingKey.DigestDays, "9,7,8", "7,8,9")]
		public void UpdateChangedValue_StringArrayUnchanged_UpdateCommandNotExecuted(string settingKey, string oldValue, string newValue)
		{
			// setup 
			string[] oldValues = oldValue.Split(',');
			string[] newValues = newValue.Split(',');
			IDbContext dbContext = new DbContextBuilder().Build();
            IUpsertGlobalUserSettingCommand upsertGlobalUserSettingCommand = new SubstituteBuilder<IUpsertGlobalUserSettingCommand>().Build();

            // execute
            IGlobalUserSettingProvider provider = CreateProvider(upsertGlobalUserSettingCommand: upsertGlobalUserSettingCommand);
			bool result = provider.UpdateChangedValue(dbContext, settingKey, oldValues, newValues).Result;

			// assert
			Assert.That(result, Is.False);
			upsertGlobalUserSettingCommand.DidNotReceive().Execute(Arg.Any<IDbContext>(), Arg.Any<GlobalUserSettingEntity>());
		}

		[TestCase(GlobalUserSettingKey.DigestDays, "1,2,3", "1,2")]
		[TestCase(GlobalUserSettingKey.DigestDays, "7,8", "7,8,9")]
		public void UpdateChangedValue_StringArrayChanged_UpdateCommandExecuted(string settingKey, string oldValue, string newValue)
		{
			// setup 
			string[] oldValues = oldValue.Split(',');
			string[] newValues = newValue.Split(',');
			IDbContext dbContext = new DbContextBuilder().Build();
            IUpsertGlobalUserSettingCommand upsertGlobalUserSettingCommand = new SubstituteBuilder<IUpsertGlobalUserSettingCommand>().Build();

            // execute
            IGlobalUserSettingProvider provider = CreateProvider(dbContext: dbContext, upsertGlobalUserSettingCommand: upsertGlobalUserSettingCommand);
			bool result = provider.UpdateChangedValue(dbContext, settingKey, oldValues, newValues).Result;

			// assert
			Assert.That(result, Is.True);
			upsertGlobalUserSettingCommand.Received(1).Execute(dbContext, new GlobalUserSettingEntity(settingKey, JsonConvert.SerializeObject(newValues)));
		}

		[TestCase(GlobalUserSettingKey.SmtpPort)]
		public void UpdateChangedValue_IntUnchanged_UpdateCommandNotExecuted(string settingKey)
		{
			// setup 
			int newValue = Faker.RandomNumber.Next();
			IDbContext dbContext = new DbContextBuilder().Build();
            IUpsertGlobalUserSettingCommand upsertGlobalUserSettingCommand = new SubstituteBuilder<IUpsertGlobalUserSettingCommand>().Build();

            // execute
            IGlobalUserSettingProvider provider = CreateProvider(upsertGlobalUserSettingCommand: upsertGlobalUserSettingCommand);
			bool result = provider.UpdateChangedValue(dbContext, settingKey, newValue, newValue).Result;

			// assert
			Assert.That(result, Is.False);
			upsertGlobalUserSettingCommand.DidNotReceive().Execute(Arg.Any<IDbContext>(), Arg.Any<GlobalUserSettingEntity>());
		}

		[TestCase(GlobalUserSettingKey.SmtpPort)]
		public void UpdateChangedValue_IntChanged_UpdateCommandExecuted(string settingKey)
		{
			// setup 
			int newValue = Faker.RandomNumber.Next(100, 1000);
			IDbContext dbContext = new DbContextBuilder().Build();
            IUpsertGlobalUserSettingCommand upsertGlobalUserSettingCommand = new SubstituteBuilder<IUpsertGlobalUserSettingCommand>().Build();

            // execute
            IGlobalUserSettingProvider provider = CreateProvider(dbContext: dbContext, upsertGlobalUserSettingCommand: upsertGlobalUserSettingCommand);
			bool result = provider.UpdateChangedValue(dbContext, settingKey, 1, newValue).Result;

			// assert
			Assert.That(result, Is.True);
			upsertGlobalUserSettingCommand.Received(1).Execute(dbContext, new GlobalUserSettingEntity(settingKey, newValue.ToString()));
		}

		[TestCase(GlobalUserSettingKey.SmtpEnableSsl)]
		public void UpdateChangedValue_BooleanUnchanged_UpdateCommandNotExecuted(string settingKey)
		{
			// setup 
			bool newValue = (Faker.RandomNumber.Next(1, 2) == 1);
			IDbContext dbContext = new DbContextBuilder().Build();
            IUpsertGlobalUserSettingCommand upsertGlobalUserSettingCommand = new SubstituteBuilder<IUpsertGlobalUserSettingCommand>().Build();

            // execute
            IGlobalUserSettingProvider provider = CreateProvider(upsertGlobalUserSettingCommand: upsertGlobalUserSettingCommand);
			bool result = provider.UpdateChangedValue(dbContext, settingKey, newValue, newValue).Result;

			// assert
			Assert.That(result, Is.False);
			upsertGlobalUserSettingCommand.DidNotReceive().Execute(Arg.Any<IDbContext>(), Arg.Any<GlobalUserSettingEntity>());
		}

		[TestCase(GlobalUserSettingKey.SmtpEnableSsl)]
		public void UpdateChangedValue_BooleanChanged_UpdateCommandExecuted(string settingKey)
		{
			// setup 
			bool newValue = true;
			IDbContext dbContext = new DbContextBuilder().Build();
            IUpsertGlobalUserSettingCommand upsertGlobalUserSettingCommand = new SubstituteBuilder<IUpsertGlobalUserSettingCommand>().Build();

            // execute
            IGlobalUserSettingProvider provider = CreateProvider(dbContext: dbContext, upsertGlobalUserSettingCommand: upsertGlobalUserSettingCommand);
			bool result = provider.UpdateChangedValue(dbContext, settingKey, !newValue, newValue).Result;

			// assert
			Assert.That(result, Is.True);
			upsertGlobalUserSettingCommand.Received(1).Execute(dbContext, new GlobalUserSettingEntity(settingKey, newValue.ToString()));
		}

		#endregion


		private IGlobalUserSettingProvider CreateProvider(IDbContextFactory? dbContextFactory = null
			, IDbContext? dbContext = null
			, IEncryptionProvider? encryptionProvider = null
			, IUpsertGlobalUserSettingCommand? upsertGlobalUserSettingCommand = null
			, GlobalUserSettingRepository? globalUserSettingRepo = null)
		{
			dbContextFactory ??= Substitute.For<IDbContextFactory>();
			encryptionProvider ??= Substitute.For<IEncryptionProvider>();
			upsertGlobalUserSettingCommand ??= Substitute.For<IUpsertGlobalUserSettingCommand>();
			dbContext ??= Substitute.For<IDbContext>();
			dbContextFactory.GetDbContext().Returns(dbContext);

			globalUserSettingRepo ??= Substitute.For<GlobalUserSettingRepository>();
			IGlobalUserSettingProvider provider = new GlobalUserSettingProvider(Substitute.For<ILogger<GlobalUserSettingProvider>>(), dbContextFactory, encryptionProvider, upsertGlobalUserSettingCommand, globalUserSettingRepo);
			return provider;
		}

		private void SetupStoredValue(IDbContext dbContext, GlobalUserSettingRepository settingRepo, string settingId, string storedSettingValue)
		{
			List<GlobalUserSettingEntity> settingEntities = new List<GlobalUserSettingEntity>();
			settingEntities.Add(new GlobalUserSettingEntity(settingId, storedSettingValue));
			settingRepo.GetAllAsync(dbContext).Returns(Task.FromResult(settingEntities.AsEnumerable()));

		}


	}
}
