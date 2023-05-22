using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.ViewOrchestrators.Api.Update;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UpdateController : ControllerBase
    {
        private readonly IUpdateCheckOrchestrator _udateCheckOrchestrator;
        private readonly IUpdateInstallOrchestrator _updateInstallOrchestrator;

        public UpdateController(IUpdateCheckOrchestrator updateCheckOrchestrator, IUpdateInstallOrchestrator updateInstallOrchestrator)
        {
            _udateCheckOrchestrator = updateCheckOrchestrator;
            _updateInstallOrchestrator = updateInstallOrchestrator;
        }

        [HttpGet("check")]
        public async Task<IActionResult> Check()
        {
            return await _udateCheckOrchestrator.Execute();
        }

        [HttpPost("install")]
        public async Task<IActionResult> Install()
        {
            return await _updateInstallOrchestrator.Execute();
        }

    }
}
