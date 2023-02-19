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
using SftpSchedulerService.Models;
using SftpSchedulerService.ViewProviders.Host;
using Microsoft.AspNetCore.Authorization;

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HostController : ControllerBase
    {
        private readonly HostCreateProvider _hostCreateProvider;
        private readonly HostFetchAllProvider _hostFetchAllProvider;

        public HostController(HostCreateProvider hostCreateProvider, HostFetchAllProvider hostFetchAllProvider)
        {
            _hostCreateProvider = hostCreateProvider;
            _hostFetchAllProvider = hostFetchAllProvider;
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
