using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.User;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Identity;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace SftpScheduler.BLL.Tests.Identity
{
    [TestFixture]
    public class IdentityInitialiserTests
    {
        [TestCase(UserRoles.Admin)]
        [TestCase(UserRoles.User)]
        public void Seed_RoleExists_DoesNotRecreate(string roleName)
        {
            ILogger<IdentityInitialiser> logger = Substitute.For<ILogger<IdentityInitialiser>>();
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            RoleManager<IdentityRole> roleManager = IdentityTestHelper.CreateRoleManagerMock();
            ICreateUserCommand createUserCommand = Substitute.For<ICreateUserCommand>();

            roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(Task.FromResult(true));
            roleManager.RoleExistsAsync(roleName).Returns(Task.FromResult(true));

            IdentityInitialiser identityInitialiser = new IdentityInitialiser(logger, userManager, roleManager, createUserCommand);
            identityInitialiser.Seed();

            roleManager.Received(1).RoleExistsAsync(roleName);
            roleManager.DidNotReceive().CreateAsync(Arg.Any<IdentityRole>());


        }

        [TestCase(UserRoles.Admin)]
        [TestCase(UserRoles.User)]
        public void Seed_RoleDoesNotExist_IsCreated(string roleName)
        {
            ILogger<IdentityInitialiser> logger = Substitute.For<ILogger<IdentityInitialiser>>();
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            RoleManager<IdentityRole> roleManager = IdentityTestHelper.CreateRoleManagerMock();
            ICreateUserCommand createUserCommand = Substitute.For<ICreateUserCommand>();

            roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(Task.FromResult(true));
            roleManager.RoleExistsAsync(roleName).Returns(Task.FromResult(false));
            roleManager.CreateAsync(Arg.Any<IdentityRole>()).Returns(Task.FromResult(IdentityResult.Success));

            IdentityInitialiser identityInitialiser = new IdentityInitialiser(logger, userManager, roleManager, createUserCommand);
            identityInitialiser.Seed();

            roleManager.Received(1).CreateAsync(Arg.Any<IdentityRole>());
        }

        [Test]
        public void Seed_RoleFailsToCreate_ThrowsDataValidationException()
        {
            ILogger<IdentityInitialiser> logger = Substitute.For<ILogger<IdentityInitialiser>>();
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            RoleManager<IdentityRole> roleManager = IdentityTestHelper.CreateRoleManagerMock();
            ICreateUserCommand createUserCommand = Substitute.For<ICreateUserCommand>();

            roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(Task.FromResult(false));
            roleManager.CreateAsync(Arg.Any<IdentityRole>()).Returns(Task.FromResult(IdentityResult.Failed(new IdentityError() {  Code = "test", Description = "test" })));

            IdentityInitialiser identityInitialiser = new IdentityInitialiser(logger, userManager, roleManager, createUserCommand);
            try
            {
                identityInitialiser.Seed();
                Assert.Fail("DataValidation not thrown");
            }
            catch (DataValidationException dve)
            {
                Assert.That(dve.ValidationResult.ErrorMessages[0], Is.EqualTo("test"));
            }
        }

        [Test]
        public void Seed_AdminUserDoesNotExist_IsCreated()
        {
            ILogger<IdentityInitialiser> logger = Substitute.For<ILogger<IdentityInitialiser>>();
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            RoleManager<IdentityRole> roleManager = IdentityTestHelper.CreateRoleManagerMock();
            ICreateUserCommand createUserCommand = Substitute.For<ICreateUserCommand>();

            roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(Task.FromResult(true));
            userManager.FindByNameAsync(IdentityInitialiser.DefaultAdminUserName).Returns(Task.FromResult<UserEntity>(null));

            IdentityInitialiser identityInitialiser = new IdentityInitialiser(logger, userManager, roleManager, createUserCommand);
            identityInitialiser.Seed();

            userManager.Received(1).FindByNameAsync(IdentityInitialiser.DefaultAdminUserName);

            createUserCommand.Received(1).ExecuteAsync(userManager
                , Arg.Any<UserEntity>()
                , IdentityInitialiser.DefaultAdminUserPassword, Arg.Any<IEnumerable<string>>()).GetAwaiter().GetResult();

        }

        [Test]
        public void Seed_AdminUserExists_IsNotRecreated()
        {
            ILogger<IdentityInitialiser> logger = Substitute.For<ILogger<IdentityInitialiser>>();
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            RoleManager<IdentityRole> roleManager = IdentityTestHelper.CreateRoleManagerMock();
            ICreateUserCommand createUserCommand = Substitute.For<ICreateUserCommand>();

            UserEntity user = new UserEntity();

            roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(Task.FromResult(true));
            userManager.FindByNameAsync(IdentityInitialiser.DefaultAdminUserName).Returns(Task.FromResult(user));

            IdentityInitialiser identityInitialiser = new IdentityInitialiser(logger, userManager, roleManager, createUserCommand);
            identityInitialiser.Seed();

            userManager.Received(1).FindByNameAsync(IdentityInitialiser.DefaultAdminUserName);

            createUserCommand.DidNotReceive().ExecuteAsync(Arg.Any<UserManager<UserEntity>>()
                , Arg.Any<UserEntity>()
                , Arg.Any<string>()
                , Arg.Any<IEnumerable<string>>()).GetAwaiter().GetResult();
        }

    }
}
