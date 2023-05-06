using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.ViewOrchestrators.Api.User;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserFetchAllOrchestrator _userFetchAllOrchestrator;

        public UsersController(IUserFetchAllOrchestrator userFetchAllOrchestrator)
        {
            _userFetchAllOrchestrator = userFetchAllOrchestrator;
        }

        [HttpGet()]
        public async Task<IActionResult> Get()
        {
            return await _userFetchAllOrchestrator.Execute();
        }

    }
}
