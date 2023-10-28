using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.User;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Tests.Builders.Identity;
using System.Security.Claims;

namespace SftpScheduler.BLL.Tests.Commands.User
{
    [TestFixture]
    public class ChangePasswordCommandTests
    {
        [Test]
        public void ExecuteAsync_ResultFails_ThrowsDataValidationException()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal();
            string currentPassword = Guid.NewGuid().ToString();
            string newPassword = Guid.NewGuid().ToString();

            userManager.ChangePasswordAsync(Arg.Any<UserEntity>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Failed()));

            ChangePasswordCommand changePasswordCmd = new ChangePasswordCommand();

            try
            {
                changePasswordCmd.ExecuteAsync(userManager, claimsPrincipal, currentPassword, newPassword).GetAwaiter().GetResult();
                Assert.Fail("DataValidationException not thrown as expected");
            }
            catch (DataValidationException)
            {
                // this is expected, yay
            }

        }

        [Test]
        public void ExecuteAsync_ResultSucceeds_PasswordIsChanged()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal();
            string currentPassword = Guid.NewGuid().ToString();
            string newPassword = Guid.NewGuid().ToString();

            UserEntity userEntity = new UserEntity();
            userManager.GetUserAsync(claimsPrincipal).Returns(userEntity);

            userManager.ChangePasswordAsync(Arg.Any<UserEntity>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Success));

            ChangePasswordCommand changePasswordCmd = new ChangePasswordCommand();
            changePasswordCmd.ExecuteAsync(userManager, claimsPrincipal, currentPassword, newPassword).GetAwaiter().GetResult();

            userManager.Received(1).GetUserAsync(claimsPrincipal);
            userManager.Received(1).ChangePasswordAsync(userEntity, currentPassword, newPassword);
        }

        [Test]
        public void ExecuteAsync_ResultSucceeds_ReturnsCreatedUser()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal();
            string currentPassword = Guid.NewGuid().ToString();
            string newPassword = Guid.NewGuid().ToString();

            UserEntity userEntity = new UserEntity();
            userManager.GetUserAsync(claimsPrincipal).Returns(userEntity);

            userManager.ChangePasswordAsync(Arg.Any<UserEntity>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Success));

            ChangePasswordCommand changePasswordCmd = new ChangePasswordCommand();
            UserEntity result = changePasswordCmd.ExecuteAsync(userManager, claimsPrincipal, currentPassword, newPassword).GetAwaiter().GetResult();
            Assert.That(result.Id, Is.EqualTo(userEntity.Id));
        }

    }
}
