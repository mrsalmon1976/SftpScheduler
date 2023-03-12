using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.ViewOrchestrators.Api.Job;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly JobCreateOrchestrator _jobCreateProvider;
        private readonly JobFetchAllOrchestrator _jobFetchAllProvider;

        public JobsController(JobCreateOrchestrator jobCreateProvider, JobFetchAllOrchestrator jobFetchAllProvider)
        {
            _jobCreateProvider = jobCreateProvider;
            _jobFetchAllProvider = jobFetchAllProvider;
        }

        //// GET: api/<ApiAuthController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await _jobFetchAllProvider.Execute();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] JobViewModel model)
        {
            return await _jobCreateProvider.Execute(model);
        }

        //// PUT api/<ApiAuthController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<ApiAuthController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}

    }
}
