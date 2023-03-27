﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.ViewOrchestrators.Api.JobLog;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/jobs/{jobId}/logs")]
    [ApiController]
    public class JobLogsController : ControllerBase
    {
        private readonly JobLogFetchAllOrchestrator _jobLogFetchAllOrchestrator;

        public JobLogsController(JobLogFetchAllOrchestrator jobLogFetchAllOrchestrator)
        {
            _jobLogFetchAllOrchestrator = jobLogFetchAllOrchestrator;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogsByJobId([FromRoute] string jobId, [FromQuery] int? maxLogId)
        {
            return await _jobLogFetchAllOrchestrator.Execute(jobId, maxLogId);
        }


    }
}
