using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Identity.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SftpSchedulerService.Config;
using System.Text;
using SftpScheduler.BLL.Models;

namespace SftpSchedulerService.ViewOrchestrators.Api.Login
{
    public interface ILoginPostOrchestrator : IViewOrchestrator
    {
        Task<IActionResult> Execute(LoginModel model, HttpContext httpContext);
    }

    public class LoginPostOrchestrator : ILoginPostOrchestrator
    {
        private readonly ILogger<LoginPostOrchestrator> _logger;
        private readonly AppSettings _appSettings;
        private readonly UserManager<UserEntity> _userManager;

        public LoginPostOrchestrator(ILogger<LoginPostOrchestrator> logger, AppSettings appSettings, UserManager<UserEntity> userManager)
        {
            _logger = logger;
            _appSettings = appSettings;
            _userManager = userManager;
        }

        public async Task<IActionResult> Execute(LoginModel model, HttpContext httpContext)
        {
            // check if the user exists
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                _logger.LogWarning($"Attempt to log in with unknown user '{model.Username}'");
                return new UnauthorizedResult();
            }

            // check password is correct
            bool isPasswordOk = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordOk)
            {
                _logger.LogWarning($"Bad password supplied for user '{model.Username}'");
                return new UnauthorizedResult();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtSecurityToken = GetToken(authClaims);
            string token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            //Response.Cookies.Append("token", token);
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaims(authClaims);
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(1)
                });

            return new OkObjectResult(new
            {
                token = token,
                expiration = jwtSecurityToken.ValidTo
            });
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
