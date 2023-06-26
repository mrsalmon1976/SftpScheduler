using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.User;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;
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
		public void ExecuteAsync_CreateDefaultAdminAccount_NoDataValidation()
		{
            // setup
			string userName = Guid.NewGuid().ToString();
			string password = Guid.NewGuid().ToString();

			UserEntity userEntity = new UserEntity();
			userEntity.UserName = IdentityInitialiser.DefaultAdminUserName;

			IUserValidator userValidator = Substitute.For<IUserValidator>();

			UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
			userManager.CreateAsync(Arg.Any<UserEntity>(), password).Returns(Task.FromResult(IdentityResult.Success));

			// execute
			ICreateUserCommand createUserCommand = CreateCommand(userValidator);
			createUserCommand.ExecuteAsync(userManager, userEntity, password, Enumerable.Empty<string>()).GetAwaiter().GetResult();

            // assert
			userValidator.DidNotReceive().Validate(Arg.Any<UserManager<UserEntity>>(), Arg.Any<UserEntity>());

		}

		[Test]
        public void ExecuteAsync_ValidationFails_ThrowsDataValidationException()
        {
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            string userName = Guid.NewGuid().ToString();
            string password = Guid.NewGuid().ToString();

            UserEntity userEntity = new UserEntity();
            userEntity.UserName = userName;

            IUserValidator userValidator = Substitute.For<IUserValidator>();
            userValidator.Validate(userManager, userEntity).Returns(new ValidationResult("failed"));

            ICreateUserCommand createUserCommand = CreateCommand(userValidator);

            try
            {
                createUserCommand.ExecuteAsync(userManager, userEntity, password, Enumerable.Empty<string>()).GetAwaiter().GetResult();
                Assert.Fail("DataValidationException not thrown as expected");
            }
            catch (DataValidationException)
            {
                // this is expected, yay
            }

            userManager.DidNotReceive().CreateAsync(Arg.Any<UserEntity>(), Arg.Any<string>());
            userValidator.Received(1).Validate(userManager, userEntity);

        }

        [Test]
        public void ExecuteAsync_ResultFails_ThrowsDataValidationException()
        {
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            string userName = Guid.NewGuid().ToString();
            string password = Guid.NewGuid().ToString();

            userManager.CreateAsync(Arg.Any<UserEntity>(), password).Returns(Task.FromResult(IdentityResult.Failed()));

            ICreateUserCommand createUserCommand = CreateCommand();

            UserEntity userEntity = new UserEntity();
            userEntity.UserName = userName;

            try
            {
                createUserCommand.ExecuteAsync(userManager, userEntity, password, Enumerable.Empty<string>()).GetAwaiter().GetResult();
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

            UserEntity userEntity = new UserEntity();
            userEntity.UserName = userName;

            ICreateUserCommand createUserCommand = CreateCommand();

            createUserCommand.ExecuteAsync(userManager, userEntity, password, roles).GetAwaiter().GetResult();
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

            UserEntity userEntity = new UserEntity();
            userEntity.UserName = userName;

            ICreateUserCommand createUserCommand = CreateCommand();

            UserEntity result = createUserCommand.ExecuteAsync(userManager, userEntity, password, roles).GetAwaiter().GetResult();
            Assert.That(userName, Is.EqualTo(result.UserName));
        }

        private ICreateUserCommand CreateCommand(IUserValidator? userValidator = null)
        {
            if (userValidator == null)
            {
                userValidator = Substitute.For<IUserValidator>();
                userValidator.Validate(Arg.Any<UserManager<UserEntity>>(), Arg.Any<UserEntity>()).Returns(new ValidationResult());
            }
            return new CreateUserCommand(userValidator);
        }

    }
}
