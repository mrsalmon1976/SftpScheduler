using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.ViewOrchestrators.Api.Cron;
using SftpSchedulerService.ViewOrchestrators.Api.Dashboard;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardFetchAllOrchestrator _dashboardFetchAllOrchestrator;

        public DashboardController(IDashboardFetchAllOrchestrator dashboardFetchAllOrchestrator)
        {
            _dashboardFetchAllOrchestrator = dashboardFetchAllOrchestrator;
        }

        [HttpGet]
        [Route("stats")]
        public async Task<IActionResult> Get()
        {
            return await _dashboardFetchAllOrchestrator.Execute();
        }

    }
}
