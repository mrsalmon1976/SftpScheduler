using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Config;
using SftpSchedulerService.Config;
using SftpSchedulerService.Models.Settings;

namespace SftpSchedulerService.ViewOrchestrators.Api.Settings
{
    public interface ISettingsFetchAllOrchestrator : IViewOrchestrator
    {
        IActionResult Execute();
    }

    public class SettingsFetchAllOrchestrator : ISettingsFetchAllOrchestrator
    {
		private readonly IGlobalUserSettingProvider _globalUserSettingProvider;
		private readonly IStartupSettingProvider _startupSettingProvider;

		public SettingsFetchAllOrchestrator(IGlobalUserSettingProvider globalUserSettingProvider, IStartupSettingProvider startupSettingProvider)
        {
			_globalUserSettingProvider = globalUserSettingProvider;
			_startupSettingProvider = startupSettingProvider;
		}

        public IActionResult Execute()
        {
            GlobalSettingsViewModel viewModel = new GlobalSettingsViewModel();
			SettingsModelMapper.MapValues(_globalUserSettingProvider, viewModel);
			SettingsModelMapper.MapValues(_startupSettingProvider.Load(), viewModel);
            return new OkObjectResult(viewModel);
        }
    }
}
