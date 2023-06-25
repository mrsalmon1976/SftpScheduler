using NSubstitute;
using SftpScheduler.BLL.Commands.Notification;
using SftpScheduler.BLL.Commands.Setting;
using SftpScheduler.BLL.Config;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.TestInfrastructure.Commands.Notification;
using SftpScheduler.BLL.TestInfrastructure.Config;
using SftpScheduler.BLL.TestInfrastructure.Data;
using SftpSchedulerService.Config;
using SftpSchedulerService.Models.Settings;
using SftpSchedulerService.TestInfrastructure.Config;
using SftpSchedulerService.TestInfrastructure.Models.Settings;
using SftpSchedulerService.ViewOrchestrators.Api.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Settings
{
	[TestFixture]
	public class SettingsUpdateOrchestratorTests
	{
		[Test]
		public void Execute_StartupSettingsChanged_Saved()
		{
			// setup
			StartupSettings startupSettings = new StartupSettingsBuilder().WithMaxConcurrentJobs(2).Build();
			IStartupSettingProvider startupSettingProvider = new StartupSettingProviderBuilder().WithLoadReturns(startupSettings).Build();
			GlobalSettingsViewModel globalSettingsViewModel = new GlobalSettingsViewModelBuilder().WithMaxConcurrentJobs(3).Build();

			// execute
			var orchestrator = CreateOrchestrator(startupSettingProvider: startupSettingProvider);
			orchestrator.Execute(globalSettingsViewModel);

			// assert
			startupSettingProvider.Received(1).Save(Arg.Any<StartupSettings>());
		}

		[Test]
		public void Execute_StartupSettingsNotChanged_NotSaved()
		{
			// setup
			int maxConcurrentJobs = Faker.RandomNumber.Next(1, 10);
			StartupSettings startupSettings = new StartupSettingsBuilder().WithMaxConcurrentJobs(maxConcurrentJobs).Build();
			IStartupSettingProvider startupSettingProvider = new StartupSettingProviderBuilder().WithLoadReturns(startupSettings).Build();
			GlobalSettingsViewModel globalSettingsViewModel = new GlobalSettingsViewModelBuilder().WithMaxConcurrentJobs(maxConcurrentJobs).Build();

			// execute
			var orchestrator = CreateOrchestrator(startupSettingProvider: startupSettingProvider);
			orchestrator.Execute(globalSettingsViewModel);

			// assert
			startupSettingProvider.DidNotReceive().Save(Arg.Any<StartupSettings>());
		}


		[Test]
		public void Execute_DigestTimeChanged_IsUpdated()
		{
			// setup
			IDbContext dbContext = new DbContextBuilder().Build();
			IStartupSettingProvider startupSettingProvider = new StartupSettingProviderBuilder().WithLoadReturns(new StartupSettings()).Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder().WithDigestTime(1).Build();
			GlobalSettingsViewModel globalSettingsViewModel = new GlobalSettingsViewModelBuilder().WithDigestTime(2).Build();

			// execute
			var orchestrator = CreateOrchestrator(dbContext: dbContext, globalUserSettingProvider: globalUserSettingProvider, startupSettingProvider: startupSettingProvider);
			orchestrator.Execute(globalSettingsViewModel);

			// assert
			globalUserSettingProvider.Received(1).UpdateChangedValue(dbContext, GlobalUserSettingKey.DigestTime, globalUserSettingProvider.DigestTime, globalSettingsViewModel.DigestTime);
		}

		[Test]
		public void Execute_DigestTimeChanged_DigestJobUpdated()
		{
			// setup
			int digestTimeNew = Faker.RandomNumber.Next(2, 23);
			int digestTimeOld = digestTimeNew - 1;
			IDbContext dbContext = new DbContextBuilder().Build();
			IStartupSettingProvider startupSettingProvider = new StartupSettingProviderBuilder().WithLoadReturns(new StartupSettings()).Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder()
				.WithDigestTime(digestTimeOld)
				.WithUpdateChangedValueReturns(dbContext, GlobalUserSettingKey.DigestTime, digestTimeOld, digestTimeNew, true)
				.Build();
			GlobalSettingsViewModel globalSettingsViewModel = new GlobalSettingsViewModelBuilder().WithDigestTime(digestTimeNew).Build();
			IUpsertDigestCommand upsertDigestCommand = new UpsertDigestCommandBuilder().Build();

			// execute
			var orchestrator = CreateOrchestrator(dbContext: dbContext, globalUserSettingProvider: globalUserSettingProvider, startupSettingProvider: startupSettingProvider, upsertDigestCommand: upsertDigestCommand);
			orchestrator.Execute(globalSettingsViewModel);

			// assert
			upsertDigestCommand.Received(1).Execute(Arg.Any<IEnumerable<DayOfWeek>>(), digestTimeNew);
		}

		[Test]
		public void Execute_DigestDaysChanged_IsUpdated()
		{
			// setup
			DayOfWeek[] digestDaysNew = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Friday };
			string[] digestDaysNewString = digestDaysNew.Select(x => x.ToString()).ToArray();
			DayOfWeek[] digestDaysOld = { DayOfWeek.Monday, DayOfWeek.Tuesday };
			string[] digestDaysOldString = digestDaysOld.Select(x => x.ToString()).ToArray();
			IDbContext dbContext = new DbContextBuilder().Build();
			IStartupSettingProvider startupSettingProvider = new StartupSettingProviderBuilder().WithLoadReturns(new StartupSettings()).Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder()
				.WithDigestDays(digestDaysOld)
				.WithUpdateChangedValueReturns(dbContext, GlobalUserSettingKey.DigestDays, Arg.Any<string[]>(), Arg.Any<string[]>(), true)
				.Build();
			GlobalSettingsViewModel globalSettingsViewModel = new GlobalSettingsViewModelBuilder().WithDigestDays(digestDaysNew).Build();
			IUpsertDigestCommand upsertDigestCommand = new UpsertDigestCommandBuilder().Build();

			// execute
			var orchestrator = CreateOrchestrator(dbContext: dbContext, globalUserSettingProvider: globalUserSettingProvider, startupSettingProvider: startupSettingProvider, upsertDigestCommand: upsertDigestCommand);
			orchestrator.Execute(globalSettingsViewModel);

			// assert
			globalUserSettingProvider.Received(1).UpdateChangedValue(dbContext, GlobalUserSettingKey.DigestDays, Arg.Any<string[]>(), Arg.Any<string[]>());
		}

		[Test]
		public void Execute_DigestDaysChanged_DigestJobUpdated()
		{
			// setup
			DayOfWeek[] digestDaysNew = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Friday };
			DayOfWeek[] digestDaysOld = { DayOfWeek.Monday, DayOfWeek.Tuesday };
			IDbContext dbContext = new DbContextBuilder().Build();
			IStartupSettingProvider startupSettingProvider = new StartupSettingProviderBuilder().WithLoadReturns(new StartupSettings()).Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder()
				.WithDigestDays(digestDaysOld)
				.WithUpdateChangedValueReturns(dbContext, GlobalUserSettingKey.DigestDays, Arg.Any<string[]>(), Arg.Any<string[]>(), true)
				.Build();
			GlobalSettingsViewModel globalSettingsViewModel = new GlobalSettingsViewModelBuilder().WithDigestDays(digestDaysNew).Build();
			IUpsertDigestCommand upsertDigestCommand = new UpsertDigestCommandBuilder().Build();

			// execute
			var orchestrator = CreateOrchestrator(dbContext: dbContext, globalUserSettingProvider: globalUserSettingProvider, startupSettingProvider: startupSettingProvider, upsertDigestCommand: upsertDigestCommand);
			orchestrator.Execute(globalSettingsViewModel);

			// assert
			upsertDigestCommand.Received(1).Execute(Arg.Any<IEnumerable<DayOfWeek>>(), Arg.Any<int>());
		}

		[Test]
		public void Execute_DigestTimeAndDigestDaysNotChanged_DigestJobNotUpdated()
		{
			// setup
			IDbContext dbContext = new DbContextBuilder().Build();
			IStartupSettingProvider startupSettingProvider = new StartupSettingProviderBuilder().WithLoadReturns(new StartupSettings()).Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder().Build();
			GlobalSettingsViewModel globalSettingsViewModel = new GlobalSettingsViewModelBuilder().Build();
			IUpsertDigestCommand upsertDigestCommand = new UpsertDigestCommandBuilder().Build();

			// execute
			var orchestrator = CreateOrchestrator(dbContext: dbContext, globalUserSettingProvider: globalUserSettingProvider, startupSettingProvider: startupSettingProvider, upsertDigestCommand: upsertDigestCommand);
			orchestrator.Execute(globalSettingsViewModel);

			// assert
			upsertDigestCommand.DidNotReceive().Execute(Arg.Any<IEnumerable<DayOfWeek>>(), Arg.Any<int>());
		}

		[Test]
		public void Execute_SmtpHostChanged_IsUpdated()
		{
			// setup
			string smtpHostNew = Guid.NewGuid().ToString();
			string smtpHostOld = Guid.NewGuid().ToString(); ;
			IDbContext dbContext = new DbContextBuilder().Build();
			IStartupSettingProvider startupSettingProvider = new StartupSettingProviderBuilder().WithLoadReturns(new StartupSettings()).Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder().WithSmtpHost(smtpHostOld).Build();
			GlobalSettingsViewModel globalSettingsViewModel = new GlobalSettingsViewModelBuilder().WithSmtpHost(smtpHostNew).Build();

			// execute
			var orchestrator = CreateOrchestrator(dbContext: dbContext, globalUserSettingProvider: globalUserSettingProvider, startupSettingProvider: startupSettingProvider);
			orchestrator.Execute(globalSettingsViewModel);

			// assert
			globalUserSettingProvider.Received(1).UpdateChangedValue(dbContext, GlobalUserSettingKey.SmtpHost, globalUserSettingProvider.SmtpHost, globalSettingsViewModel.SmtpHost);
		}

		[Test]
		public void Execute_SmtpPortChanged_IsUpdated()
		{
			// setup
			IDbContext dbContext = new DbContextBuilder().Build();
			IStartupSettingProvider startupSettingProvider = new StartupSettingProviderBuilder().WithLoadReturns(new StartupSettings()).Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder().WithSmtpPort(1).Build();
			GlobalSettingsViewModel globalSettingsViewModel = new GlobalSettingsViewModelBuilder().WithSmtpPort(2).Build();

			// execute
			var orchestrator = CreateOrchestrator(dbContext: dbContext, globalUserSettingProvider: globalUserSettingProvider, startupSettingProvider: startupSettingProvider);
			orchestrator.Execute(globalSettingsViewModel);

			// assert
			globalUserSettingProvider.Received(1).UpdateChangedValue(dbContext, GlobalUserSettingKey.SmtpPort, globalUserSettingProvider.SmtpPort, globalSettingsViewModel.SmtpPort);
		}

		[Test]
		public void Execute_SmtpUserNameChanged_IsUpdated()
		{
			// setup
			IDbContext dbContext = new DbContextBuilder().Build();
			IStartupSettingProvider startupSettingProvider = new StartupSettingProviderBuilder().WithLoadReturns(new StartupSettings()).Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder().WithSmtpUserName("oldname").Build();
			GlobalSettingsViewModel globalSettingsViewModel = new GlobalSettingsViewModelBuilder().WithSmtpUserName("newname").Build();

			// execute
			var orchestrator = CreateOrchestrator(dbContext: dbContext, globalUserSettingProvider: globalUserSettingProvider, startupSettingProvider: startupSettingProvider);
			orchestrator.Execute(globalSettingsViewModel);

			// assert
			globalUserSettingProvider.Received(1).UpdateChangedValue(dbContext, GlobalUserSettingKey.SmtpUserName, globalUserSettingProvider.SmtpUserName, globalSettingsViewModel.SmtpUserName);
		}

		[Test]
		public void Execute_SmtpPasswordChanged_IsUpdated()
		{
			// setup
			IDbContext dbContext = new DbContextBuilder().Build();
			IStartupSettingProvider startupSettingProvider = new StartupSettingProviderBuilder().WithLoadReturns(new StartupSettings()).Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder().WithSmtpPassword("oldpass").Build();
			GlobalSettingsViewModel globalSettingsViewModel = new GlobalSettingsViewModelBuilder().WithSmtpPassword("newpass").Build();

			// execute
			var orchestrator = CreateOrchestrator(dbContext: dbContext, globalUserSettingProvider: globalUserSettingProvider, startupSettingProvider: startupSettingProvider);
			orchestrator.Execute(globalSettingsViewModel);

			// assert
			globalUserSettingProvider.Received(1).UpdateChangedValue(dbContext, GlobalUserSettingKey.SmtpPassword, globalUserSettingProvider.SmtpPassword, globalSettingsViewModel.SmtpPassword);
		}

		[Test]
		public void Execute_SmtpEnableSslChanged_IsUpdated()
		{
			// setup
			IDbContext dbContext = new DbContextBuilder().Build();
			IStartupSettingProvider startupSettingProvider = new StartupSettingProviderBuilder().WithLoadReturns(new StartupSettings()).Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder().WithSmtpEnableSsl(true).Build();
			GlobalSettingsViewModel globalSettingsViewModel = new GlobalSettingsViewModelBuilder().WithSmtpEnableSsl(false).Build();

			// execute
			var orchestrator = CreateOrchestrator(dbContext: dbContext, globalUserSettingProvider: globalUserSettingProvider, startupSettingProvider: startupSettingProvider);
			orchestrator.Execute(globalSettingsViewModel);

			// assert
			globalUserSettingProvider.Received(1).UpdateChangedValue(dbContext, GlobalUserSettingKey.SmtpEnableSsl, globalUserSettingProvider.SmtpEnableSsl, globalSettingsViewModel.SmtpEnableSsl);
		}

		[Test]
		public void Execute_SmtpFromNameChanged_IsUpdated()
		{
			// setup
			IDbContext dbContext = new DbContextBuilder().Build();
			IStartupSettingProvider startupSettingProvider = new StartupSettingProviderBuilder().WithLoadReturns(new StartupSettings()).Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder().WithSmtpFromName("oldname").Build();
			GlobalSettingsViewModel globalSettingsViewModel = new GlobalSettingsViewModelBuilder().WithSmtpFromName("newname").Build();

			// execute
			var orchestrator = CreateOrchestrator(dbContext: dbContext, globalUserSettingProvider: globalUserSettingProvider, startupSettingProvider: startupSettingProvider);
			orchestrator.Execute(globalSettingsViewModel);

			// assert
			globalUserSettingProvider.Received(1).UpdateChangedValue(dbContext, GlobalUserSettingKey.SmtpFromName, globalUserSettingProvider.SmtpFromName, globalSettingsViewModel.SmtpFromName);
		}

		[Test]
		public void Execute_SmtpFromEmailChanged_IsUpdated()
		{
			// setup
			IDbContext dbContext = new DbContextBuilder().Build();
			IStartupSettingProvider startupSettingProvider = new StartupSettingProviderBuilder().WithLoadReturns(new StartupSettings()).Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new GlobalUserSettingProviderBuilder().WithSmtpFromEmail("oldemail").Build();
			GlobalSettingsViewModel globalSettingsViewModel = new GlobalSettingsViewModelBuilder().WithSmtpFromEmail("newemail").Build();

			// execute
			var orchestrator = CreateOrchestrator(dbContext: dbContext, globalUserSettingProvider: globalUserSettingProvider, startupSettingProvider: startupSettingProvider);
			orchestrator.Execute(globalSettingsViewModel);

			// assert
			globalUserSettingProvider.Received(1).UpdateChangedValue(dbContext, GlobalUserSettingKey.SmtpFromEmail, globalUserSettingProvider.SmtpFromEmail, globalSettingsViewModel.SmtpFromEmail);
		}

		private ISettingsUpdateOrchestrator CreateOrchestrator(IDbContext? dbContext = null
			, IGlobalUserSettingProvider? globalUserSettingProvider = null
			, IStartupSettingProvider? startupSettingProvider = null
			, IUpsertDigestCommand? upsertDigestCommand = null
			, IUpsertGlobalUserSettingCommand? upsertGlobalUserSettingCommand = null
			)
		{
			dbContext ??= Substitute.For<IDbContext>();
			globalUserSettingProvider ??= Substitute.For<IGlobalUserSettingProvider>();
			startupSettingProvider ??= Substitute.For<IStartupSettingProvider>();
			upsertDigestCommand ??= Substitute.For<IUpsertDigestCommand>();
			upsertGlobalUserSettingCommand ??= Substitute.For<IUpsertGlobalUserSettingCommand>();

			IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
			dbContextFactory.GetDbContext().Returns(dbContext);

			return new SettingsUpdateOrchestrator(dbContextFactory, globalUserSettingProvider, startupSettingProvider, upsertDigestCommand, upsertGlobalUserSettingCommand);
		}


	}
}
