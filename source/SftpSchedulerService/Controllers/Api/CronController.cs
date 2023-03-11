using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SftpSchedulerService.Config;
using SftpScheduler.BLL.Identity.Models;
using SftpSchedulerService.Models;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using SftpSchedulerService.Models.Cron;
using SftpSchedulerService.ViewOrchestrators.Api.Cron;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CronController : ControllerBase
    {
        private readonly CronGetScheduleOrchestrator _cronGetScheduleOrchestrator;

        public CronController(CronGetScheduleOrchestrator cronGetScheduleOrchestrator)
        {
            _cronGetScheduleOrchestrator = cronGetScheduleOrchestrator;
        }

        //// GET: api/<ApiAuthController>
        [HttpGet]
        public async Task<IActionResult> Get(string schedule)
        {
            return await _cronGetScheduleOrchestrator.Execute(schedule);
        }

    }
}
