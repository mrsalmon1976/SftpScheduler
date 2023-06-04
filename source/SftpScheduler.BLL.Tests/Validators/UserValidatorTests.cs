using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Validators;

namespace SftpScheduler.BLL.Tests.Validators
{
    [TestFixture]
    public class UserValidatorTests
    {
        [Test]
        public void Validate_ValidObject_ReturnsSuccess()
        {
            // setup
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            UserEntity userEntity = EntityTestHelper.CreateUserEntity();

            UserEntity existingUserEntity = null!;
            userManager.FindByNameAsync(userEntity.UserName).Returns(existingUserEntity!);

            // execute
            IUserValidator userValidator = CreateValidator();
            var validationResult = userValidator.Validate(userManager, userEntity).GetAwaiter().GetResult();

            // assert
            Assert.That(validationResult.IsValid);
            userManager.Received(1).FindByNameAsync(userEntity.UserName);
        }

        [Test]
        public void Validate_NullUserEntity_ThrowsException()
        {
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            UserEntity? userEntity = null;

            IUserValidator userValidator = CreateValidator();
            try
            {
                var validationResult = userValidator.Validate(userManager, userEntity!).GetAwaiter().GetResult();
                Assert.Fail("NullReferenceException not thrown");
            }
            catch (NullReferenceException)
            {
                // expected
                return;
            }
        }


        [Test]
        public void Validate_UserNameExists_ReturnsFail()
        {
            // setup
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            UserEntity userEntity = EntityTestHelper.CreateUserEntity();

            UserEntity existingUserEntity = EntityTestHelper.CreateUserEntity();
            existingUserEntity.UserName = userEntity.UserName;
            userManager.FindByNameAsync(userEntity.UserName).Returns(existingUserEntity);

            // execute
            IUserValidator userValidator = CreateValidator();
            var validationResult = userValidator.Validate(userManager, userEntity).GetAwaiter().GetResult();

            // assert
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void Validate_InvalidEmail_ReturnsFail()
        {
            // setup
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            UserEntity userEntity = EntityTestHelper.CreateUserEntity();

            UserEntity existingUserEntity = null!;
            userManager.FindByNameAsync(userEntity.UserName).Returns(existingUserEntity!);

            IEmailValidator emailValidator = Substitute.For<IEmailValidator>();
            emailValidator.IsValidEmail(userEntity.Email).Returns(false);

            // execute
            IUserValidator userValidator = CreateValidator(emailValidator);
            var validationResult = userValidator.Validate(userManager, userEntity).GetAwaiter().GetResult();

            // assert
            Assert.That(validationResult.IsValid, Is.False);
            Assert.That(validationResult.ErrorMessages.Count, Is.EqualTo(1));
            emailValidator.Received(1).IsValidEmail(userEntity.Email);
        }


        private IUserValidator CreateValidator(IEmailValidator? emailValidator = null)
        {
            if (emailValidator == null)
            {
                emailValidator = Substitute.For<IEmailValidator>();
                emailValidator.IsValidEmail(Arg.Any<string>()).Returns(true);
            }
            return new UserValidator(emailValidator);
        }

    }

}
