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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SftpSchedulerService.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly AppSettings _appSettings;

        public LoginController(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        // GET: api/<ApiAuthController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ApiAuthController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        //[Route("api/auth/login")]
        public async Task<IActionResult> Post([FromBody] LoginModel model)
        {
            // TODO: Hook up to the database!!
            //var user = await _userManager.FindByNameAsync(model.Username);
            //if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            //{
            //var userRoles = await _userManager.GetRolesAsync(user);
            var user = new IdentityUser("unauthuser");
            var userRoles = new string[] { "Admin", "User" };

            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var jwtSecurityToken = GetToken(authClaims);
            string token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            //Response.Cookies.Append("token", token);
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaims(authClaims);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(1)
                });

            return Ok(new
            {
                token = token,
                expiration = jwtSecurityToken.ValidTo
            });
            //}
            //return Unauthorized();

        }

        // PUT api/<ApiAuthController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ApiAuthController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JwtSecret));

            var token = new JwtSecurityToken(
                issuer: _appSettings.JwtValidIssuer,
                audience: _appSettings.JwtValidAudience,
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}
