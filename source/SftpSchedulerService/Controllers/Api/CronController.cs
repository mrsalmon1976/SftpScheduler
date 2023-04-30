using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.ViewOrchestrators.Api.Cron;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CronController : ControllerBase
    {
        private readonly ICronGetScheduleOrchestrator _cronGetScheduleOrchestrator;

        public CronController(ICronGetScheduleOrchestrator cronGetScheduleOrchestrator)
        {
            _cronGetScheduleOrchestrator = cronGetScheduleOrchestrator;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string schedule)
        {
            return await _cronGetScheduleOrchestrator.Execute(schedule);
        }

    }
}
