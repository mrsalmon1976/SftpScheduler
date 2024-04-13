using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SftpScheduler.BLL.Config;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Config;
using SftpSchedulerService.Models.Settings;
using SftpSchedulerService.ViewOrchestrators.Api.Settings;

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Settings
{
	[TestFixture]
	public class SettingsFetchAllOrchestratorTests
	{
		[Test]
		public void Execute_CorrectlyMapsSettings()
		{
			// setup
			StartupSettings startupSettings = new SubstituteBuilder<StartupSettings>().WithRandomProperties().Build();
			IGlobalUserSettingProvider globalUserSettingProvider = new SubstituteBuilder<IGlobalUserSettingProvider>().WithRandomProperties().Build();
            IStartupSettingProvider startupSettingProvider = new SubstituteBuilder<IStartupSettingProvider>().Build();
            startupSettingProvider.Load().Returns(startupSettings);

            // execute
            ISettingsFetchAllOrchestrator orchestrator = CreateOrchestrator(globalUserSettingProvider, startupSettingProvider);
			IActionResult result = orchestrator.Execute();
			GlobalSettingsViewModel viewModel = ((result as OkObjectResult)!.Value as GlobalSettingsViewModel)!;

			// assert
			Assert.That(viewModel, Is.Not.Null);
			Assert.That(viewModel.MaxConcurrentJobs, Is.EqualTo(startupSettings.MaxConcurrentJobs));
			Assert.That(viewModel.SmtpHost, Is.EqualTo(globalUserSettingProvider.SmtpHost));
			Assert.That(viewModel.SmtpPort, Is.EqualTo(globalUserSettingProvider.SmtpPort));
		}

		private ISettingsFetchAllOrchestrator CreateOrchestrator(IGlobalUserSettingProvider? globalUserSettingProvider = null, IStartupSettingProvider? startupSettingProvider = null)
		{
			globalUserSettingProvider ??= Substitute.For<IGlobalUserSettingProvider>();
			startupSettingProvider ??= Substitute.For<IStartupSettingProvider>();
			return new SettingsFetchAllOrchestrator(globalUserSettingProvider, startupSettingProvider);
		}
	}
}
