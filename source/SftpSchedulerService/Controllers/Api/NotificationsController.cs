using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.ViewOrchestrators.Api.Job;

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
