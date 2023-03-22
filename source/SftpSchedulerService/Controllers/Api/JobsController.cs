using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using SftpScheduler.BLL.Identity;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly JobCreateOrchestrator _jobCreateOrchestrator;
        private readonly JobDeleteOneOrchestrator _jobDeleteOneOrchestrator;
        private readonly JobFetchAllOrchestrator _jobFetchAllOrchestrator;
        private readonly JobFetchOneOrchestrator _jobFetchOneOrchestrator;

        public JobsController(JobCreateOrchestrator jobCreateOrchestrator, JobDeleteOneOrchestrator jobDeleteOneOrchestrator, JobFetchAllOrchestrator jobFetchAllOrchestrator, JobFetchOneOrchestrator jobFetchOneOrchestrator)
        {
            _jobCreateOrchestrator = jobCreateOrchestrator;
            _jobDeleteOneOrchestrator = jobDeleteOneOrchestrator;
            _jobFetchAllOrchestrator = jobFetchAllOrchestrator;
            _jobFetchOneOrchestrator = jobFetchOneOrchestrator;
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
            return await _jobCreateOrchestrator.Execute(model);
        }

        //// PUT api/<ApiAuthController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        // DELETE api/<ApiAuthController>/5
        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            return await _jobDeleteOneOrchestrator.Execute(id);
           
        }

    }
}
