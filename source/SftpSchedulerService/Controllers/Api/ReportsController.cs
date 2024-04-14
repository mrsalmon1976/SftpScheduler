using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.ViewOrchestrators.Api.Report;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportJobsWithNoTransfersOrchestrator _reportJobsWithNoTransferOrchestrator;

        public ReportsController(IReportJobsWithNoTransfersOrchestrator reportJobsWithNoTransferOrchestrator
            )
        {
            _reportJobsWithNoTransferOrchestrator = reportJobsWithNoTransferOrchestrator;
        }

        //// GET: api/reports/notransfers
        [HttpGet()]
        [Route("notransfers")]
        public async Task<IActionResult> Get([FromQuery]DateTime startDate, [FromQuery]DateTime endDate)
        {
            return await _reportJobsWithNoTransferOrchestrator.Execute(startDate, endDate);
        }

    }
}
