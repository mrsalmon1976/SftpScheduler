using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SftpScheduler.BLL.Identity.Models;
using SftpSchedulerService.Config;
using SftpSchedulerService.ViewOrchestrators.Api.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8625

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Login
{
    [TestFixture]
    public class LoginPostOrchestratorTests
    {
        [Test]
        public void Execute_UserDoesNotExist_ReturnsUnauthorized()
        {
            UserManager<IdentityUser> userManager = IdentityTestHelper.CreateUserManagerMock();
            HttpContext httpContext = Substitute.For<HttpContext>();

            LoginModel loginModel = new LoginModel();
            loginModel.Username = Guid.NewGuid().ToString();
            //userManager.FindByNameAsync(Arg.Any<string>()).ReturnsNull<Task<IdentityUser>>();
            userManager.FindByNameAsync(Arg.Any<string>()).Returns(Task.FromResult<IdentityUser>(null));

            LoginPostOrchestrator loginPostOrchestrator = CreateLoginPostOrchestrator(userManager);
            IActionResult result = loginPostOrchestrator.Execute(loginModel, httpContext).GetAwaiter().GetResult();

            Assert.IsInstanceOf(typeof(UnauthorizedResult), result);
            userManager.Received(1).FindByNameAsync(loginModel.Username);
            userManager.DidNotReceive().CheckPasswordAsync(Arg.Any<IdentityUser>(), Arg.Any<string>());
        }

        [Test]
        public void Execute_PasswordIncorrect_ReturnsUnauthorized()
        {
            UserManager<IdentityUser> userManager = IdentityTestHelper.CreateUserManagerMock();
            HttpContext httpContext = Substitute.For<HttpContext>();

            IdentityUser user = new IdentityUser();
            LoginModel loginModel = new LoginModel();
            loginModel.Username = Guid.NewGuid().ToString();
            loginModel.Password = Guid.NewGuid().ToString();
            userManager.FindByNameAsync(Arg.Any<string>()).Returns(Task.FromResult(user));
            userManager.CheckPasswordAsync(Arg.Any<IdentityUser>(), Arg.Any<string>()).Returns(Task.FromResult(false));

            LoginPostOrchestrator loginPostOrchestrator = CreateLoginPostOrchestrator(userManager);
            IActionResult result = loginPostOrchestrator.Execute(loginModel, httpContext).GetAwaiter().GetResult();

            Assert.IsInstanceOf(typeof(UnauthorizedResult), result);
            userManager.Received(1).CheckPasswordAsync(Arg.Any<IdentityUser>(), loginModel.Password);
            userManager.DidNotReceive().GetRolesAsync(Arg.Any<IdentityUser>());
        }

        //[Test]
        //public void Execute_LoginSuccessful_ReturnsOkWithToken()
        //{
        //    UserManager<IdentityUser> userManager = IdentityTestHelper.CreateUserManagerMock();
        //    HttpContext httpContext = Substitute.For<HttpContext>();

        //    LoginModel loginModel = new LoginModel();
        //    loginModel.Username = Guid.NewGuid().ToString();
        //    loginModel.Password = Guid.NewGuid().ToString();

        //    IdentityUser user = new IdentityUser();
        //    user.UserName = loginModel.Username;

        //    userManager.FindByNameAsync(Arg.Any<string>()).Returns(Task.FromResult(user));
        //    userManager.CheckPasswordAsync(Arg.Any<IdentityUser>(), loginModel.Password).Returns(Task.FromResult(true));

        //    LoginPostOrchestrator loginPostOrchestrator = CreateLoginPostOrchestrator(userManager);
        //    IActionResult result = loginPostOrchestrator.Execute(loginModel, httpContext).GetAwaiter().GetResult();

        //    Assert.IsInstanceOf(typeof(OkResult), result);
        //    httpContext.Received(1).SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme
        //        , Arg.Any<ClaimsPrincipal>()
        //        , Arg.Any<AuthenticationProperties>());
        //}

        //[Test]
        //public void Execute_LoginSuccessful_SetsUserDetailClaims()
        //{
        //    Assert.Ignore();
        //}

        //[Test]
        //public void Execute_LoginSuccessful_SetsUserRoleClaims()
        //{
        //    Assert.Fail();
        //}

        private LoginPostOrchestrator CreateLoginPostOrchestrator(UserManager<IdentityUser> userManager)
        {
            ILogger<LoginPostOrchestrator> logger = Substitute.For<ILogger<LoginPostOrchestrator>>();
            AppSettings appSettings = Substitute.For<AppSettings>(Substitute.For<IConfiguration>(), String.Empty);
            appSettings.JwtSecret.Returns(Guid.NewGuid().ToString());   

            return new LoginPostOrchestrator(logger, appSettings, userManager);

        }


    }
}
