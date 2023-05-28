using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Identity.Models;
using SftpSchedulerService.Models.Auth;
using SftpSchedulerService.ViewOrchestrators.Api.Auth;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ILoginPostOrchestrator _loginPostOrchestrator;
        private readonly IChangePasswordPostOrchestrator _changePasswordPostOrchestrator;

        public AuthController(ILogger<AuthController> logger, ILoginPostOrchestrator loginPostOrchestrator, IChangePasswordPostOrchestrator changePasswordPostOrchestrator)
        {
            _logger = logger;
            _loginPostOrchestrator = loginPostOrchestrator;
            _changePasswordPostOrchestrator = changePasswordPostOrchestrator;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            return await _loginPostOrchestrator.Execute(model, this.HttpContext);
        }

        [HttpPost]
        [Route("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            return await _changePasswordPostOrchestrator.ExecuteAsync(model, this.User);
        }

    }
}
