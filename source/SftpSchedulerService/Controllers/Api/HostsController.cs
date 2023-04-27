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
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using SftpSchedulerService.Models.Host;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HostsController : ControllerBase
    {
        private readonly HostCreateOrchestrator _hostCreateProvider;
        private readonly HostFetchAllOrchestrator _hostFetchAllProvider;
        private readonly HostDeleteOneOrchestrator _hostDeleteOneOrchestrator;
        private readonly HostFingerprintScanOrchestrator _hostFingerprintScanOrchestrator;

        public HostsController(HostCreateOrchestrator hostCreateProvider, HostFetchAllOrchestrator hostFetchAllProvider, HostDeleteOneOrchestrator hostDeleteOneOrchestrator, HostFingerprintScanOrchestrator hostFingerprintScanOrchestrator)
        {
            _hostCreateProvider = hostCreateProvider;
            _hostFetchAllProvider = hostFetchAllProvider;
            _hostDeleteOneOrchestrator = hostDeleteOneOrchestrator;
            _hostFingerprintScanOrchestrator = hostFingerprintScanOrchestrator;
        }

        //// GET: api/<ApiAuthController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await _hostFetchAllProvider.Execute();
        }

        //// GET api/<ApiAuthController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        [HttpPost]
        //[Route("api/auth/login")]
        public async Task<IActionResult> Post([FromBody] HostViewModel model)
        {
            return await _hostCreateProvider.Execute(model);
        }

        [HttpPost("scanfingerprint")]
        //[Route("api/hosts/scanfingerprint")]
        public async Task<IActionResult> ScanFingerprint([FromBody] HostViewModel model)
        {
            return await _hostFingerprintScanOrchestrator.Execute(model);
            // return await _hostCreateProvider.Execute(model);
        }

        //// PUT api/<ApiAuthController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            return await _hostDeleteOneOrchestrator.Execute(id);

        }

    }
}
