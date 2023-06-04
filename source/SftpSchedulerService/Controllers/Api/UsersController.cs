using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SftpSchedulerService.ViewOrchestrators.Api.User;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.Models.User;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.Utilities;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserFetchAllOrchestrator _userFetchAllOrchestrator;
        private readonly IUserCreateOrchestrator _userCreateOrchestrator;
        private readonly IUserFetchOneOrchestrator _userFetchOneOrchestrator;
        private readonly IUserUpdateOrchestrator _userUpdateOrchestrator;

        public UsersController(IUserFetchAllOrchestrator userFetchAllOrchestrator
            , IUserCreateOrchestrator userCreateOrchestrator
            , IUserFetchOneOrchestrator userFetchOneOrchestrator
            , IUserUpdateOrchestrator userUpdateOrchestrator
            )
        {
            _userFetchAllOrchestrator = userFetchAllOrchestrator;
            _userCreateOrchestrator = userCreateOrchestrator;
            _userFetchOneOrchestrator = userFetchOneOrchestrator;
            _userUpdateOrchestrator = userUpdateOrchestrator;
        }

        [HttpGet()]
        public async Task<IActionResult> Get()
        {
            return await _userFetchAllOrchestrator.Execute();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            return await _userFetchOneOrchestrator.Execute(id);
        }


        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserViewModel model)
        {
            return await _userCreateOrchestrator.Execute(model);
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [Route("{id}")]
        public async Task<IActionResult> Post([FromBody] UserViewModel model, [FromRoute] string id)
        {
            return await _userUpdateOrchestrator.Execute(model);
        }


    }
}
