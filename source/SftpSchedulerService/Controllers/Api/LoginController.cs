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
using SftpSchedulerService.ViewOrchestrators.Api.Login;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly ILoginPostOrchestrator _loginPostOrchestrator;
        private readonly AppSettings _appSettings;
        private readonly UserManager<IdentityUser> _userManager;

        public LoginController(ILogger<LoginController> logger, ILoginPostOrchestrator loginPostOrchestrator, AppSettings appSettings, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _loginPostOrchestrator = loginPostOrchestrator;
            _appSettings = appSettings;
            _userManager = userManager;
        }

        //// GET: api/<ApiAuthController>
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<ApiAuthController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        [HttpPost]
        //[Route("api/auth/login")]
        public async Task<IActionResult> Post([FromBody] LoginModel model)
        {
            return await _loginPostOrchestrator.Execute(model, this.HttpContext);
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
