using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.Utilities;
using SftpSchedulerService.ViewOrchestrators.Api.JobAuditLog;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly IJobCreateOrchestrator _jobCreateOrchestrator;
        private readonly IJobDeleteOneOrchestrator _jobDeleteOneOrchestrator;
        private readonly IJobFetchAllOrchestrator _jobFetchAllOrchestrator;
        private readonly IJobFetchOneOrchestrator _jobFetchOneOrchestrator;
        private readonly IJobUpdateOrchestrator _jobUpdateOrchestrator;
        private readonly IJobExecuteOrchestrator _jobExecuteOrchestrator;
        private readonly IJobAuditLogFetchAllOrchestrator _jobAuditLogFetchAllOrchestrator;

        public JobsController(IJobCreateOrchestrator jobCreateOrchestrator
            , IJobDeleteOneOrchestrator jobDeleteOneOrchestrator
            , IJobFetchAllOrchestrator jobFetchAllOrchestrator
            , IJobFetchOneOrchestrator jobFetchOneOrchestrator
            , IJobUpdateOrchestrator jobUpdateOrchestrator
            , IJobExecuteOrchestrator jobExecuteOrchestrator
            , IJobAuditLogFetchAllOrchestrator jobAuditLogFetchAllOrchestrator
            )
        {
            _jobCreateOrchestrator = jobCreateOrchestrator;
            _jobDeleteOneOrchestrator = jobDeleteOneOrchestrator;
            _jobFetchAllOrchestrator = jobFetchAllOrchestrator;
            _jobFetchOneOrchestrator = jobFetchOneOrchestrator;
            _jobUpdateOrchestrator = jobUpdateOrchestrator;
            _jobExecuteOrchestrator = jobExecuteOrchestrator;
            _jobAuditLogFetchAllOrchestrator = jobAuditLogFetchAllOrchestrator;
        }

        //// GET: api/jobs
        [HttpGet()]
        public async Task<IActionResult> Get()
        {
            return await _jobFetchAllOrchestrator.Execute();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            return await _jobFetchOneOrchestrator.Execute(id);
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] JobViewModel model)
        {
            return await _jobCreateOrchestrator.Execute(model, User.Identity!.Name!);
        }


        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [Route("{id}")]
        public async Task<IActionResult> Post([FromBody] JobViewModel model, [FromRoute] string id)
        {
            model.Id = UrlUtils.Decode(id);
            return await _jobUpdateOrchestrator.Execute(model, User.Identity!.Name!);
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [Route("{id}/run")]
        public async Task<IActionResult> Post([FromRoute] string id)
        {
            return await _jobExecuteOrchestrator.Execute(id);
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            return await _jobDeleteOneOrchestrator.Execute(id);
           
        }

        [HttpGet]
        [Route("{id}/auditlogs")]
        public async Task<IActionResult> GetAuditLogs([FromRoute] string id)
        {
            return await _jobAuditLogFetchAllOrchestrator.Execute(id);
        }


    }
}
