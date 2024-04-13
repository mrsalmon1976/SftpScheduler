using NSubstitute;
using SftpScheduler.BLL.Commands.Notification;
using SftpScheduler.BLL.Commands.Setting;
using SftpScheduler.BLL.Config;
using SftpScheduler.BLL.Data;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Config;
using SftpSchedulerService.Models.Settings;
using SftpSchedulerService.ViewOrchestrators.Api.Settings;

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Settings
{
	[TestFixture]
	public class SettingsUpdateOrchestratorTests
	{
		[Test]
		public void Execute_StartupSettingsChanged_Saved()
		{
			// setup
            StartupSettings startupSettings = new SubstituteBuilder<StartupSettings>().WithProperty(x => x.MaxConcurrentJobs, 2).Build();

            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
			startupSettingProvider.Load().Returns(startupSettings);

            GlobalSettingsViewModel globalSettingsViewModel = new SubstituteBuilder<GlobalSettingsViewModel>()
				.WithProperty(x => x.MaxConcurrentJobs, 3).Build();

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
			StartupSettings startupSettings = new SubstituteBuilder<StartupSettings>().WithProperty(x => x.MaxConcurrentJobs, maxConcurrentJobs).Build();
            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
            startupSettingProvider.Load().Returns(startupSettings);
            GlobalSettingsViewModel globalSettingsViewModel = new SubstituteBuilder<GlobalSettingsViewModel>()
                .WithProperty(x => x.MaxConcurrentJobs, maxConcurrentJobs).Build();

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
			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
			startupSettingProvider.Load().Returns(new StartupSettings());

            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
                .WithProperty(x => x.DigestTime, 1)
                .Build();

            GlobalSettingsViewModel globalSettingsViewModel = new SubstituteBuilder<GlobalSettingsViewModel>().WithProperty(x => x.DigestTime, 2).Build();

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
			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
            startupSettingProvider.Load().Returns(new StartupSettings());
            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
				.WithProperty(x => x.DigestTime, digestTimeOld)
				.Build();
			globalUserSettingProvider.UpdateChangedValue(dbContext, GlobalUserSettingKey.DigestTime, digestTimeOld, digestTimeNew).Returns(true);

            GlobalSettingsViewModel globalSettingsViewModel = new SubstituteBuilder<GlobalSettingsViewModel>()
                .WithProperty(x => x.DigestTime, digestTimeNew).Build();

			IUpsertDigestCommand upsertDigestCommand = new SubstituteBuilder<IUpsertDigestCommand>().Build();

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
			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
            startupSettingProvider.Load().Returns(new StartupSettings());

            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
                .WithProperty(x => x.DigestDays, digestDaysOld)
                .Build();
			globalUserSettingProvider.UpdateChangedValue(dbContext, GlobalUserSettingKey.DigestDays, Arg.Any<string[]>(), Arg.Any<string[]>()).Returns(true);
            GlobalSettingsViewModel globalSettingsViewModel = new SubstituteBuilder<GlobalSettingsViewModel>()
                .WithProperty(x => x.DigestDays, digestDaysNewString).Build();

            IUpsertDigestCommand upsertDigestCommand = new SubstituteBuilder<IUpsertDigestCommand>().Build();

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
            string[] digestDaysNewString = digestDaysNew.Select(x => x.ToString()).ToArray();
            DayOfWeek[] digestDaysOld = { DayOfWeek.Monday, DayOfWeek.Tuesday };
			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
            startupSettingProvider.Load().Returns(new StartupSettings());
            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
                .WithProperty(x => x.DigestDays, digestDaysOld)
                .Build();
            globalUserSettingProvider.UpdateChangedValue(dbContext, GlobalUserSettingKey.DigestDays, Arg.Any<string[]>(), Arg.Any<string[]>()).Returns(true);

            GlobalSettingsViewModel globalSettingsViewModel = new SubstituteBuilder<GlobalSettingsViewModel>()
                .WithProperty(x => x.DigestDays, digestDaysNewString).Build();
            IUpsertDigestCommand upsertDigestCommand = new SubstituteBuilder<IUpsertDigestCommand>().Build();

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
			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
            startupSettingProvider.Load().Returns(new StartupSettings());
            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>().Build();

            GlobalSettingsViewModel globalSettingsViewModel = new SubstituteBuilder<GlobalSettingsViewModel>().Build();
            IUpsertDigestCommand upsertDigestCommand = new SubstituteBuilder<IUpsertDigestCommand>().Build();

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
			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
            startupSettingProvider.Load().Returns(new StartupSettings());
            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
				.WithProperty(x => x.SmtpHost, smtpHostOld)
				.Build();
            GlobalSettingsViewModel globalSettingsViewModel = new SubstituteBuilder<GlobalSettingsViewModel>().WithProperty(x => x.SmtpHost, smtpHostNew).Build();

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
			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
            startupSettingProvider.Load().Returns(new StartupSettings());
            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
                .WithProperty(x => x.SmtpPort, 1)
                .Build();
            GlobalSettingsViewModel globalSettingsViewModel = new SubstituteBuilder<GlobalSettingsViewModel>().WithProperty(x => x.SmtpPort, 2).Build();

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
			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
            startupSettingProvider.Load().Returns(new StartupSettings());
            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
                .WithProperty(x => x.SmtpUserName, "oldname")
                .Build();
            GlobalSettingsViewModel globalSettingsViewModel = new SubstituteBuilder<GlobalSettingsViewModel>().WithProperty(x => x.SmtpUserName, "newname").Build();

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
			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
            startupSettingProvider.Load().Returns(new StartupSettings());
            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
                .WithProperty(x => x.SmtpPassword, "oldpass")
                .Build();

            GlobalSettingsViewModel globalSettingsViewModel = new SubstituteBuilder<GlobalSettingsViewModel>().WithProperty(x => x.SmtpPassword, "newpass").Build();

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
            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
            startupSettingProvider.Load().Returns(new StartupSettings());
            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
                .WithProperty(x => x.SmtpEnableSsl, true)
                .Build();

            GlobalSettingsViewModel globalSettingsViewModel = new SubstituteBuilder<GlobalSettingsViewModel>().WithProperty(x => x.SmtpEnableSsl, false).Build();

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
			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
            startupSettingProvider.Load().Returns(new StartupSettings());
            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
                .WithProperty(x => x.SmtpFromName, "oldname")
                .Build();
            GlobalSettingsViewModel globalSettingsViewModel = new SubstituteBuilder<GlobalSettingsViewModel>().WithProperty(x => x.SmtpFromName, "newname").Build();

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
			IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
            startupSettingProvider.Load().Returns(new StartupSettings());
            IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>()
                .WithProperty(x => x.SmtpFromEmail, "oldemail")
                .Build();

            GlobalSettingsViewModel globalSettingsViewModel = new SubstituteBuilder<GlobalSettingsViewModel>().WithProperty(x => x.SmtpFromEmail, "newemail").Build();

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
