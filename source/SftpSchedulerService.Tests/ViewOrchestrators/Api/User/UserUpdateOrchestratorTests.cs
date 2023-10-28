﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.User;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.Models.User;
using SftpSchedulerService.ViewOrchestrators.Api.User;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.User
{
    [TestFixture]
    public class UserUpdateOrchestratorTests
    {
        [Test]
        public void Execute_OnDataValidationException_ReturnsBadRequest()
        {
            IUpdateUserCommand updateUserCommand = Substitute.For<IUpdateUserCommand>();
            UserViewModel userViewModel = ViewModelTestHelper.CreateUserViewModel();

            updateUserCommand.ExecuteAsync(Arg.Any<UserManager<UserEntity>>(), Arg.Any<UserEntity>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>()).Throws(new DataValidationException("exception", new ValidationResult(new string[] { "error" })));

            IUserUpdateOrchestrator userCreateOrchestrator = CreateOrchestrator(updateUserCommand: updateUserCommand);
            var result = userCreateOrchestrator.Execute(userViewModel).Result as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<ValidationResult>());

        }

        [Test]
        public void Execute_OnSave_ReturnsOk()
        {
            UserViewModel userViewModel = ViewModelTestHelper.CreateUserViewModel();

            IUserUpdateOrchestrator orchestrator = CreateOrchestrator();
            var result = orchestrator.Execute(userViewModel).Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<UserViewModel>());
        }

        [Test]
        public void Execute_OnSave_ReturnsResultWithId()
        {
            UserViewModel userViewModel = ViewModelTestHelper.CreateUserViewModel();
            UserEntity userEntity = new UserEntityBuilder().WithRandomProperties().Build();

            IUpdateUserCommand updateUserCmd = Substitute.For<IUpdateUserCommand>();
            updateUserCmd.ExecuteAsync(Arg.Any<UserManager<UserEntity>>(), Arg.Any<UserEntity>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>()).Returns(userEntity);

            IUserUpdateOrchestrator orchestrator = CreateOrchestrator(updateUserCommand: updateUserCmd);
            var result = orchestrator.Execute(userViewModel).Result as OkObjectResult;

            UserViewModel resultModel = result.Value as UserViewModel;
            Assert.That(resultModel, Is.Not.Null);
            Assert.That(resultModel.Id, Is.Not.EqualTo(userViewModel.Id));

            Guid gval;
            if (!Guid.TryParse(resultModel.Id, out gval))
            {
                Assert.Fail("User view model Id not set to Guid value");
            }
        }

        private IUserUpdateOrchestrator CreateOrchestrator(UserManager<UserEntity>? userManager = null, IUpdateUserCommand? updateUserCommand = null)
        {
            userManager ??= IdentityTestHelper.CreateUserManagerMock();
            updateUserCommand ??= Substitute.For<IUpdateUserCommand>();

            return new UserUpdateOrchestrator(userManager, updateUserCommand);

        }

    }
}
