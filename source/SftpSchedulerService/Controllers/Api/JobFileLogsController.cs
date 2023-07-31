using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.ViewOrchestrators.Api.JobFileLog;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/jobs/{jobId}/filelogs")]
    [ApiController]
    [Authorize]
    public class JobFileLogsController : ControllerBase
    {
        private readonly IJobFileLogFetchAllOrchestrator _jobFileLogFetchAllOrchestrator;

        public JobFileLogsController(IJobFileLogFetchAllOrchestrator jobFileLogFetchAllOrchestrator)
        {
            _jobFileLogFetchAllOrchestrator = jobFileLogFetchAllOrchestrator;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogsByJobId([FromRoute] string jobId, [FromQuery] int? maxLogId)
        {
            return await _jobFileLogFetchAllOrchestrator.Execute(jobId, maxLogId);
        }


    }
}
