using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.Utilities;
using SftpSchedulerService.ViewOrchestrators.Api.Host;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IJobNotificationFetchAllOrchestrator _jobNotificationFetchAllOrchestrator;

        public NotificationsController(IJobNotificationFetchAllOrchestrator jobNotificationFetchAllOrchestrator)
        {
            _jobNotificationFetchAllOrchestrator = jobNotificationFetchAllOrchestrator;
        }

        [HttpGet()]
        [Route("jobs")]
        public async Task<IActionResult> GetJobNotifications([FromQuery]bool forceReload = false)
        {
            return await _jobNotificationFetchAllOrchestrator.Execute(forceReload);
        }


    }
}
