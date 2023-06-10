using Microsoft.AspNetCore.Mvc;
using SftpSchedulerService.Config;
using SftpSchedulerService.Models.Settings;

namespace SftpSchedulerService.ViewOrchestrators.Api.Settings
{
    public interface ISettingsFetchAllOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute();
    }

    public class SettingsFetchAllOrchestrator : ISettingsFetchAllOrchestrator
    {
        private readonly IGlobalSettingsProvider _globalSettingsProvider;

        public SettingsFetchAllOrchestrator(IGlobalSettingsProvider globalSettingsProvider)
        {
            _globalSettingsProvider = globalSettingsProvider;
        }

        public async Task<IActionResult> Execute()
        {
            var result = GlobalSettingMapper.MapToViewModel(_globalSettingsProvider.Load());
            return new OkObjectResult(result);
        }
    }
}
