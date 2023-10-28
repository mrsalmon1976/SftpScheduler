using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.User;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Tests.Builders.Identity;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpScheduler.BLL.Validators;

namespace SftpScheduler.BLL.Tests.Commands.User
{
    [TestFixture]
    public class UpdateUserCommandTests
    {
        [Test]
        public void ExecuteAsync_ValidationFails_ThrowsDataValidationException()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            UserEntity userEntity = new UserEntityBuilder().WithRandomProperties().Build();

            IUserValidator userValidator = Substitute.For<IUserValidator>();
            userValidator.Validate(userManager, userEntity).Returns(new ValidationResult("failed"));

            IUpdateUserCommand cmd = CreateCommand(userValidator);

            try
            {
                cmd.ExecuteAsync(userManager, userEntity, String.Empty, Enumerable.Empty<string>()).GetAwaiter().GetResult();
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
        public void ExecuteAsync_UpdateUserFails_ThrowsDataValidationException()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            UserEntity userEntity = new UserEntityBuilder().WithRandomProperties().Build();
            userManager.FindByIdAsync(userEntity.Id).Returns(userEntity);

            userManager.UpdateAsync(Arg.Any<UserEntity>()).Returns(Task.FromResult(IdentityResult.Failed()));

            IUpdateUserCommand cmd = CreateCommand();

            try
            {
                cmd.ExecuteAsync(userManager, userEntity, String.Empty, Enumerable.Empty<string>()).GetAwaiter().GetResult();
                Assert.Fail("DataValidationException not thrown as expected");
            }
            catch (DataValidationException)
            {
                // this is expected, yay
            }

            userManager.Received(1).UpdateAsync(userEntity);

        }

        [Test]
        public void ExecuteAsync_RolesUnchanged_NoUpdatesApplied()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            UserEntity userEntity = new UserEntityBuilder().WithRandomProperties().Build();
            userManager.FindByIdAsync(userEntity.Id).Returns(userEntity);

            string[] existingRoles = { UserRoles.User };
            string[] newRoles = { UserRoles.User };

            userManager.GetRolesAsync(Arg.Any<UserEntity>()).Returns(existingRoles);
            userManager.UpdateAsync(Arg.Any<UserEntity>()).Returns(Task.FromResult(IdentityResult.Success));
            userManager.RemoveFromRolesAsync(Arg.Any<UserEntity>(), Arg.Any<IEnumerable<string>>()).Returns(Task.FromResult(IdentityResult.Success));

            IUpdateUserCommand cmd = CreateCommand();

            cmd.ExecuteAsync(userManager, userEntity, String.Empty, newRoles).GetAwaiter().GetResult();

            userManager.DidNotReceive().RemoveFromRolesAsync(Arg.Any<UserEntity>(), Arg.Any<IEnumerable<string>>());
            userManager.DidNotReceive().AddToRolesAsync(Arg.Any<UserEntity>(), Arg.Any<IEnumerable<string>>());
        }

        [Test]
        public void ExecuteAsync_RoleRemoved_UpdatesCorrectly()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            UserEntity userEntity = new UserEntityBuilder().WithRandomProperties().Build();
            userManager.FindByIdAsync(userEntity.Id).Returns(userEntity);

            string[] existingRoles = { UserRoles.Admin, UserRoles.User };
            string[] newRoles = { UserRoles.User};
            
            userManager.GetRolesAsync(userEntity).Returns(existingRoles);
            userManager.UpdateAsync(Arg.Any<UserEntity>()).Returns(Task.FromResult(IdentityResult.Success));
            userManager.RemoveFromRolesAsync(Arg.Any<UserEntity>(), Arg.Any<IEnumerable<string>>()).Returns(Task.FromResult(IdentityResult.Success));

            IUpdateUserCommand cmd = CreateCommand();

            cmd.ExecuteAsync(userManager, userEntity, String.Empty, newRoles).GetAwaiter().GetResult();

            userManager.Received(1).RemoveFromRolesAsync(userEntity, Arg.Any<IEnumerable<string>>());
            userManager.DidNotReceive().AddToRolesAsync(Arg.Any<UserEntity>(), Arg.Any<IEnumerable<string>>());
        }

        [Test]
        public void ExecuteAsync_RoleRemovalFails_ThrowsException()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            UserEntity userEntity = new UserEntityBuilder().WithRandomProperties().Build();
            userManager.FindByIdAsync(userEntity.Id).Returns(userEntity);

            string[] existingRoles = { UserRoles.Admin, UserRoles.User };
            string[] newRoles = { UserRoles.User };

            userManager.GetRolesAsync(userEntity).Returns(existingRoles);
            userManager.UpdateAsync(Arg.Any<UserEntity>()).Returns(Task.FromResult(IdentityResult.Success));
            userManager.RemoveFromRolesAsync(Arg.Any<UserEntity>(), Arg.Any<IEnumerable<string>>()).Returns(Task.FromResult(IdentityResult.Failed()));

            IUpdateUserCommand cmd = CreateCommand();

            try
            {
                cmd.ExecuteAsync(userManager, userEntity, String.Empty, newRoles).GetAwaiter().GetResult();
                Assert.Fail("DataValidationException not thrown as expected");
            }
            catch (DataValidationException)
            {
                // expected exception
            }
            userManager.Received(1).RemoveFromRolesAsync(userEntity, Arg.Any<IEnumerable<string>>());

        }

        [Test]
        public void ExecuteAsync_RoleAdded_UpdatesCorrectly()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            UserEntity userEntity = new UserEntityBuilder().WithRandomProperties().Build();
            userManager.FindByIdAsync(userEntity.Id).Returns(userEntity);

            string[] existingRoles = { UserRoles.User };
            string[] newRoles = { UserRoles.Admin, UserRoles.User };

            userManager.GetRolesAsync(userEntity).Returns(existingRoles);
            userManager.UpdateAsync(Arg.Any<UserEntity>()).Returns(Task.FromResult(IdentityResult.Success));
            userManager.AddToRolesAsync(userEntity, Arg.Any<IEnumerable<string>>()).Returns(Task.FromResult(IdentityResult.Success));

            IUpdateUserCommand cmd = CreateCommand();

            cmd.ExecuteAsync(userManager, userEntity, String.Empty, newRoles).GetAwaiter().GetResult();

            userManager.DidNotReceive().RemoveFromRolesAsync(Arg.Any<UserEntity>(), Arg.Any<IEnumerable<string>>());
            userManager.Received(1).AddToRolesAsync(userEntity, Arg.Any<IEnumerable<string>>());
        }

        [Test]
        public void ExecuteAsync_RoleAdditionFails_ThrowsException()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            UserEntity userEntity = new UserEntityBuilder().WithRandomProperties().Build();
            userManager.FindByIdAsync(userEntity.Id).Returns(userEntity);

            string[] existingRoles = { UserRoles.User };
            string[] newRoles = { UserRoles.Admin, UserRoles.User };

            userManager.GetRolesAsync(userEntity).Returns(existingRoles);
            userManager.UpdateAsync(Arg.Any<UserEntity>()).Returns(Task.FromResult(IdentityResult.Success));
            userManager.AddToRolesAsync(userEntity, Arg.Any<IEnumerable<string>>()).Returns(Task.FromResult(IdentityResult.Failed()));

            IUpdateUserCommand cmd = CreateCommand();

            try
            {
                cmd.ExecuteAsync(userManager, userEntity, String.Empty, newRoles).GetAwaiter().GetResult();
                Assert.Fail("DataValidationException not thrown as expected");
            }
            catch (DataValidationException)
            {
                // expected exception
            }
            userManager.Received(1).AddToRolesAsync(userEntity, Arg.Any<IEnumerable<string>>());

        }

        [Test]
        public void ExecuteAsync_PasswordSupplied_UpdatesPasswordCorrectly()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            UserEntity userEntity = new UserEntityBuilder().WithRandomProperties().Build();
            userManager.FindByIdAsync(userEntity.Id).Returns(userEntity);
            string password = Guid.NewGuid().ToString();
            string token = Guid.NewGuid().ToString();

            string[] roles = { UserRoles.User };

            userManager.GetRolesAsync(userEntity).Returns(roles);
            userManager.UpdateAsync(Arg.Any<UserEntity>()).Returns(Task.FromResult(IdentityResult.Success));
            userManager.GeneratePasswordResetTokenAsync(userEntity).Returns(token);
            userManager.ResetPasswordAsync(userEntity, token, password).Returns(Task.FromResult(IdentityResult.Success));

            IUpdateUserCommand cmd = CreateCommand();
            cmd.ExecuteAsync(userManager, userEntity, password, roles).GetAwaiter().GetResult();

            // assert
            userManager.Received(1).GeneratePasswordResetTokenAsync(userEntity);
            userManager.Received(1).ResetPasswordAsync(userEntity, token, password);

        }

        [Test]
        public void ExecuteAsync_PasswordSuppliedButFails_ThrowsException()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            UserEntity userEntity = new UserEntityBuilder().WithRandomProperties().Build();
            userManager.FindByIdAsync(userEntity.Id).Returns(userEntity);
            string password = Guid.NewGuid().ToString();
            string token = Guid.NewGuid().ToString();

            string[] roles = { UserRoles.User };

            userManager.GetRolesAsync(userEntity).Returns(roles);
            userManager.UpdateAsync(Arg.Any<UserEntity>()).Returns(Task.FromResult(IdentityResult.Success));
            userManager.GeneratePasswordResetTokenAsync(Arg.Any<UserEntity>()).Returns(token);
            userManager.ResetPasswordAsync(Arg.Any<UserEntity>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Failed()));

            IUpdateUserCommand cmd = CreateCommand();
            try
            {
                cmd.ExecuteAsync(userManager, userEntity, password, roles).GetAwaiter().GetResult();
                Assert.Fail("DataValidationException not thrown as expected");
            }
            catch (DataValidationException)
            {
                // expected exception
            }

            // assert
            userManager.Received(1).GeneratePasswordResetTokenAsync(userEntity);
            userManager.Received(1).ResetPasswordAsync(userEntity, token, password);

        }

        [Test]
        public void ExecuteAsync_SuccessfulUpdate_ReturnsUpdatedUserEntity()
        {
            UserManager<UserEntity> userManager = new UserManagerBuilder().Build();
            UserEntity userEntity = new UserEntityBuilder().WithRandomProperties().Build();
            userManager.FindByIdAsync(userEntity.Id).Returns(userEntity);

            string[] roles = { UserRoles.User };

            userManager.GetRolesAsync(Arg.Any<UserEntity>()).Returns(roles);
            userManager.UpdateAsync(Arg.Any<UserEntity>()).Returns(Task.FromResult(IdentityResult.Success));

            IUpdateUserCommand cmd = CreateCommand();
            UserEntity result = cmd.ExecuteAsync(userManager, userEntity, String.Empty, roles).GetAwaiter().GetResult();

            // assert
            Assert.That(result.Id, Is.EqualTo(userEntity.Id));
            userManager.Received(1).UpdateAsync(userEntity);
        }


        private IUpdateUserCommand CreateCommand(IUserValidator? userValidator = null)
        {
            if (userValidator == null)
            {
                userValidator = Substitute.For<IUserValidator>();
                userValidator.Validate(Arg.Any<UserManager<UserEntity>>(), Arg.Any<UserEntity>()).Returns(new ValidationResult());
            }
            return new UpdateUserCommand(userValidator);
        }

    }
}
