using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HostsController : ControllerBase
    {
        private readonly IHostCreateOrchestrator _hostCreateOrchestrator;
        private readonly IHostFetchAllOrchestrator _hostFetchAllOrchestrator;
        private readonly IHostDeleteOneOrchestrator _hostDeleteOneOrchestrator;
        private readonly IHostFingerprintScanOrchestrator _hostFingerprintScanOrchestrator;
        private readonly IHostFetchOneOrchestrator _hostFetchOneOrchestrator;
        private readonly IHostUpdateOrchestrator _hostUpdateOrchestrator;

        public HostsController(IHostCreateOrchestrator hostCreateOrchestrator
            , IHostFetchAllOrchestrator hostFetchAllOrchestrator
            , IHostDeleteOneOrchestrator hostDeleteOneOrchestrator
            , IHostFingerprintScanOrchestrator hostFingerprintScanOrchestrator
            , IHostFetchOneOrchestrator hostFetchOneOrchestrator
            , IHostUpdateOrchestrator hostUpdateOrchestrator
            )
        {
            _hostCreateOrchestrator = hostCreateOrchestrator;
            _hostFetchAllOrchestrator = hostFetchAllOrchestrator;
            _hostDeleteOneOrchestrator = hostDeleteOneOrchestrator;
            _hostFingerprintScanOrchestrator = hostFingerprintScanOrchestrator;
            _hostFetchOneOrchestrator = hostFetchOneOrchestrator;
            _hostUpdateOrchestrator = hostUpdateOrchestrator;
        }

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

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        //[Route("api/auth/login")]
        public async Task<IActionResult> Post([FromBody] HostViewModel model)
        {
            return await _hostCreateOrchestrator.Execute(model);
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [Route("{id}")]
        public async Task<IActionResult> Post([FromBody] HostViewModel model, [FromRoute]string id)
        {
            model.Id = UrlUtils.Decode(id);
            return await _hostUpdateOrchestrator.Execute(model);
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost("scanfingerprint")]
        public async Task<IActionResult> ScanFingerprint([FromBody] HostViewModel model)
        {
            return await _hostFingerprintScanOrchestrator.Execute(model);
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            return await _hostDeleteOneOrchestrator.Execute(id);

        }

    }
}
