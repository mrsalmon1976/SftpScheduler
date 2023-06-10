using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.ViewOrchestrators.Api.Settings;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.Models.Settings;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = UserRoles.Admin)]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsFetchAllOrchestrator _settingsFetchAllOrchestrator;
        private readonly ISettingsUpdateOrchestrator _settingsUpdateOrchestrator;

        public SettingsController(ISettingsFetchAllOrchestrator settingsFetchAllOrchestrator, ISettingsUpdateOrchestrator settingsUpdateOrchestrator)
        {
            _settingsFetchAllOrchestrator = settingsFetchAllOrchestrator;
            _settingsUpdateOrchestrator = settingsUpdateOrchestrator;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await _settingsFetchAllOrchestrator.Execute();
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GlobalSettingsViewModel model)
        {
            return await _settingsUpdateOrchestrator.Execute(model);
        }



    }
}
