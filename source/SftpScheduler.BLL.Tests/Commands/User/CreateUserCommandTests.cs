using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.User;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.Commands.User
{
    [TestFixture]
    public class CreateUserCommandTests
    {
        [Test]
        public void ExecuteAsync_ResultFails_ThrowsDataValidationException()
        {
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            string userName = Guid.NewGuid().ToString();
            string password = Guid.NewGuid().ToString();

            userManager.CreateAsync(Arg.Any<UserEntity>(), password).Returns(Task.FromResult(IdentityResult.Failed()));

            CreateUserCommand createUserCommand= new CreateUserCommand();

            try
            {
                createUserCommand.ExecuteAsync(userManager, userName, password, Enumerable.Empty<string>()).GetAwaiter().GetResult();
                Assert.Fail("DataValidationException not thrown as expected");
            }
            catch (DataValidationException)
            {
                // this is expected, yay
            }

            userManager.Received(1).CreateAsync(Arg.Any<UserEntity>(), password);

        }

        [Test]
        public void ExecuteAsync_ResultSucceeds_RolesAdded()
        {
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            string userName = Guid.NewGuid().ToString();
            string password = Guid.NewGuid().ToString();
            string[] roles = new string[] { "Admin", "Test" };

            userManager.CreateAsync(Arg.Any<UserEntity>(), password).Returns(Task.FromResult(IdentityResult.Success));

            CreateUserCommand createUserCommand = new CreateUserCommand();

            createUserCommand.ExecuteAsync(userManager, userName, password, roles).GetAwaiter().GetResult();
            userManager.Received(1).AddToRolesAsync(Arg.Any<UserEntity>(), roles);
        }

        [Test]
        public void ExecuteAsync_ResultSucceeds_ReturnsCreatedUser()
        {
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            string userName = Guid.NewGuid().ToString();
            string password = Guid.NewGuid().ToString();
            string[] roles = new string[] { "Admin", "Test" };

            userManager.CreateAsync(Arg.Any<UserEntity>(), password).Returns(Task.FromResult(IdentityResult.Success));

            CreateUserCommand createUserCommand = new CreateUserCommand();

            UserEntity result = createUserCommand.ExecuteAsync(userManager, userName, password, roles).GetAwaiter().GetResult();
            Assert.That(userName, Is.EqualTo(result.UserName));
        }

    }
}
