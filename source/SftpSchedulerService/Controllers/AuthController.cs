using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Controllers
{
    public class AuthController : CustomBaseController
    {
        public AuthController() 
        {
        }

        [HttpGet]
        public ViewResult Login()
        {
            return this.View("Login");
        }

        [Authorize]
        [HttpGet("auth/change-password")]
        public ViewResult ChangePassword()
        {
            return this.View("ChangePassword");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            AuthenticationHttpContextExtensions.SignOutAsync(HttpContext, CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter().GetResult();
            return this.Redirect("/");
        }

    }
}
