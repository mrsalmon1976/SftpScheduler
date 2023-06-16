using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.ViewOrchestrators.Api.Settings;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.Models.Settings;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = UserRoles.Admin)]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsFetchAllOrchestrator _settingsFetchAllOrchestrator;
        private readonly ISettingsUpdateOrchestrator _settingsUpdateOrchestrator;
        private readonly ISettingsEmailTestOrchestrator _settingsEmailTestOrchestrator;

        public SettingsController(ISettingsFetchAllOrchestrator settingsFetchAllOrchestrator
            , ISettingsUpdateOrchestrator settingsUpdateOrchestrator
            , ISettingsEmailTestOrchestrator settingsEmailTestOrchestrator
            )
        {
            _settingsFetchAllOrchestrator = settingsFetchAllOrchestrator;
            _settingsUpdateOrchestrator = settingsUpdateOrchestrator;
            _settingsEmailTestOrchestrator = settingsEmailTestOrchestrator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await _settingsFetchAllOrchestrator.Execute();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GlobalSettingsViewModel model)
        {
            return await _settingsUpdateOrchestrator.Execute(model);
        }


        [HttpPost]
        [Route("email-test")]
        public async Task<IActionResult> Post([FromBody] EmailTestViewModel model)
        {
            return await _settingsEmailTestOrchestrator.Execute(model);
        }


    }
}
