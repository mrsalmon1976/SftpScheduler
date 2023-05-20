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

        public UpdateController(IUpdateCheckOrchestrator updateCheckOrchestrator)
        {
            _udateCheckOrchestrator = updateCheckOrchestrator;
        }

        [HttpGet("check")]
        public async Task<IActionResult> Check()
        {
            return await _udateCheckOrchestrator.Execute();
        }

    }
}
