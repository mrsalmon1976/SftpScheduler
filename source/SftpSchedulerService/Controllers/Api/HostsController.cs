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
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HostsController : ControllerBase
    {
        private readonly HostCreateOrchestrator _hostCreateOrchestrator;
        private readonly HostFetchAllOrchestrator _hostFetchAllOrchestrator;
        private readonly HostDeleteOneOrchestrator _hostDeleteOneOrchestrator;
        private readonly HostFingerprintScanOrchestrator _hostFingerprintScanOrchestrator;
        private readonly HostFetchOneOrchestrator _hostFetchOneOrchestrator;
        private readonly HostUpdateOrchestrator _hostUpdateOrchestrator;

        public HostsController(HostCreateOrchestrator hostCreateOrchestrator
            , HostFetchAllOrchestrator hostFetchAllOrchestrator
            , HostDeleteOneOrchestrator hostDeleteOneOrchestrator
            , HostFingerprintScanOrchestrator hostFingerprintScanOrchestrator
            , HostFetchOneOrchestrator hostFetchOneOrchestrator
            , HostUpdateOrchestrator hostUpdateOrchestrator
            )
        {
            _hostCreateOrchestrator = hostCreateOrchestrator;
            _hostFetchAllOrchestrator = hostFetchAllOrchestrator;
            _hostDeleteOneOrchestrator = hostDeleteOneOrchestrator;
            _hostFingerprintScanOrchestrator = hostFingerprintScanOrchestrator;
            _hostFetchOneOrchestrator = hostFetchOneOrchestrator;
            _hostUpdateOrchestrator = hostUpdateOrchestrator;
        }

        //// GET: api/<ApiAuthController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await _hostFetchAllOrchestrator.Execute();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            return await _hostFetchOneOrchestrator.Execute(id);
        }

        [HttpPost]
        //[Route("api/auth/login")]
        public async Task<IActionResult> Post([FromBody] HostViewModel model)
        {
            return await _hostCreateOrchestrator.Execute(model);
        }

        [HttpPost]
        [Route("{id}")]
        public async Task<IActionResult> Post([FromBody] HostViewModel model, [FromRoute]string id)
        {
            model.Id = UrlUtils.Decode(id);
            return await _hostUpdateOrchestrator.Execute(model);
        }

        [HttpPost("scanfingerprint")]
        //[Route("api/hosts/scanfingerprint")]
        public async Task<IActionResult> ScanFingerprint([FromBody] HostViewModel model)
        {
            return await _hostFingerprintScanOrchestrator.Execute(model);
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
