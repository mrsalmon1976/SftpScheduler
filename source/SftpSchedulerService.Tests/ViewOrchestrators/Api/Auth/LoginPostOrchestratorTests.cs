using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SftpScheduler.BLL.Identity.Models;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Tests.Builders.Identity;
using SftpSchedulerService.Config;
using SftpSchedulerService.ViewOrchestrators.Api.Auth;

#pragma warning disable CS8625

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Auth
{
    [TestFixture]
    public class LoginPostOrchestratorTests
    {
        [Test]
        public void Execute_UserDoesNotExist_ReturnsUnauthorized()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            HttpContext httpContext = Substitute.For<HttpContext>();

            LoginModel loginModel = new LoginModel();
            loginModel.Username = Guid.NewGuid().ToString();
            //userManager.FindByNameAsync(Arg.Any<string>()).ReturnsNull<Task<UserEntity>>();
            userManager.FindByNameAsync(Arg.Any<string>()).Returns(Task.FromResult<UserEntity>(null));

            LoginPostOrchestrator loginPostOrchestrator = CreateLoginPostOrchestrator(userManager);
            IActionResult result = loginPostOrchestrator.Execute(loginModel, httpContext).GetAwaiter().GetResult();

            Assert.IsInstanceOf(typeof(UnauthorizedResult), result);
            userManager.Received(1).FindByNameAsync(loginModel.Username);
            userManager.DidNotReceive().CheckPasswordAsync(Arg.Any<UserEntity>(), Arg.Any<string>());
        }

        [Test]
        public void Execute_PasswordIncorrect_ReturnsUnauthorized()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            HttpContext httpContext = Substitute.For<HttpContext>();

            UserEntity user = new UserEntity();
            LoginModel loginModel = new LoginModel();
            loginModel.Username = Guid.NewGuid().ToString();
            loginModel.Password = Guid.NewGuid().ToString();
            userManager.FindByNameAsync(Arg.Any<string>()).Returns(Task.FromResult(user));
            userManager.CheckPasswordAsync(Arg.Any<UserEntity>(), Arg.Any<string>()).Returns(Task.FromResult(false));

            LoginPostOrchestrator loginPostOrchestrator = CreateLoginPostOrchestrator(userManager);
            IActionResult result = loginPostOrchestrator.Execute(loginModel, httpContext).GetAwaiter().GetResult();

            Assert.IsInstanceOf(typeof(UnauthorizedResult), result);
            userManager.Received(1).CheckPasswordAsync(Arg.Any<UserEntity>(), loginModel.Password);
            userManager.DidNotReceive().IsLockedOutAsync(Arg.Any<UserEntity>());
        }

        [Test]
        public void Execute_AccountLockedOut_ReturnsUnauthorized()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            HttpContext httpContext = Substitute.For<HttpContext>();

            UserEntity user = new UserEntity();
            LoginModel loginModel = new LoginModel();
            loginModel.Username = Guid.NewGuid().ToString();
            loginModel.Password = Guid.NewGuid().ToString();
            userManager.FindByNameAsync(Arg.Any<string>()).Returns(Task.FromResult(user));
            userManager.CheckPasswordAsync(Arg.Any<UserEntity>(), Arg.Any<string>()).Returns(Task.FromResult(true));
            userManager.IsLockedOutAsync(Arg.Any<UserEntity>()).Returns(Task.FromResult(true));

            LoginPostOrchestrator loginPostOrchestrator = CreateLoginPostOrchestrator(userManager);
            IActionResult result = loginPostOrchestrator.Execute(loginModel, httpContext).GetAwaiter().GetResult();

            Assert.IsInstanceOf(typeof(UnauthorizedResult), result);
            userManager.Received(1).IsLockedOutAsync(Arg.Any<UserEntity>());
            userManager.DidNotReceive().GetRolesAsync(Arg.Any<UserEntity>());
        }

        //[Test]
        //public void Execute_LoginSuccessful_ReturnsOkWithToken()
        //{
        //    UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
        //    HttpContext httpContext = Substitute.For<HttpContext>();

        //    LoginModel loginModel = new LoginModel();
        //    loginModel.Username = Guid.NewGuid().ToString();
        //    loginModel.Password = Guid.NewGuid().ToString();

        //    UserEntity user = new UserEntity();
        //    user.UserName = loginModel.Username;

        //    userManager.FindByNameAsync(Arg.Any<string>()).Returns(Task.FromResult(user));
        //    userManager.CheckPasswordAsync(Arg.Any<UserEntity>(), loginModel.Password).Returns(Task.FromResult(true));

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

        private LoginPostOrchestrator CreateLoginPostOrchestrator(UserManager<UserEntity> userManager)
        {
            ILogger<LoginPostOrchestrator> logger = Substitute.For<ILogger<LoginPostOrchestrator>>();
            AppSettings appSettings = Substitute.For<AppSettings>(Substitute.For<IConfiguration>(), String.Empty);
            appSettings.JwtSecret.Returns(Guid.NewGuid().ToString());   

            return new LoginPostOrchestrator(logger, appSettings, userManager);

        }


    }
}
