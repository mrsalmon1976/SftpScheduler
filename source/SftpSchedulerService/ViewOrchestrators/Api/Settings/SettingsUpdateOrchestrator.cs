using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Config;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.Models.Settings;

namespace SftpSchedulerService.ViewOrchestrators.Api.Settings
{
    public interface ISettingsUpdateOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(GlobalSettingsViewModel globalSettingsViewModel);
    }

    public class SettingsUpdateOrchestrator : ISettingsUpdateOrchestrator
    {
        private readonly IGlobalSettingsProvider _globalSettingsProvider;

        public SettingsUpdateOrchestrator(IGlobalSettingsProvider globalSettingsProvider)
        {
            _globalSettingsProvider = globalSettingsProvider;
        }

        public async Task<IActionResult> Execute(GlobalSettingsViewModel globalSettingsViewModel)
        {
            var settings = GlobalSettingMapper.MapToConfig(globalSettingsViewModel);
            await _globalSettingsProvider.Save(settings);
            return new OkResult();
        }
    }
}
