using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.User;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Tests.Builders.Identity;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.Models.Auth;
using SftpSchedulerService.ViewOrchestrators.Api.Auth;
using System.Security.Claims;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Auth
{
    [TestFixture]
    public class ChangePasswordPostOrchestratorTests
    {
        [Test]
        public void Execute_OnDataValidationException_ReturnsBadRequest()
        {
            // setup
            IChangePasswordCommand changePasswordCmd = Substitute.For<IChangePasswordCommand>();
            changePasswordCmd.ExecuteAsync(Arg.Any<UserManager<UserEntity>>(), Arg.Any<ClaimsPrincipal>(), Arg.Any<string>(), Arg.Any<string>()).Throws(new DataValidationException("exception", new ValidationResult(new string[] { "error" })));

            // execute
            ChangePasswordPostOrchestrator orchestrator = CreateOrchestrator(changePasswordCommand: changePasswordCmd);
            var result = orchestrator.ExecuteAsync(new ChangePasswordViewModel(), new ClaimsPrincipal()).Result as BadRequestObjectResult;

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<ValidationResult>());

        }

        [Test]
        public void Execute_OnSave_ExecutesCommand()
        {
            // setup
            IChangePasswordCommand changePasswordCmd = Substitute.For<IChangePasswordCommand>();
            ChangePasswordViewModel viewModel = new ChangePasswordViewModel();
            viewModel.CurrentPassword = Guid.NewGuid().ToString();
            viewModel.NewPassword = Guid.NewGuid().ToString();

            // execute 
            ChangePasswordPostOrchestrator orchestrator = CreateOrchestrator(changePasswordCommand: changePasswordCmd);
            orchestrator.ExecuteAsync(viewModel, new ClaimsPrincipal()).Wait();

            // assert
            changePasswordCmd.Received(1).ExecuteAsync(Arg.Any<UserManager<UserEntity>>(), Arg.Any<ClaimsPrincipal>(), viewModel.CurrentPassword, viewModel.NewPassword);
        }

        [Test]
        public void Execute_OnSave_ReturnsOk()
        {
            // setup

            // execute 
            ChangePasswordPostOrchestrator orchestrator = CreateOrchestrator();
            var result = orchestrator.ExecuteAsync(new ChangePasswordViewModel(), new ClaimsPrincipal()).Result as OkResult;

            // assert
            Assert.That(result, Is.Not.Null);
        }

        private ChangePasswordPostOrchestrator CreateOrchestrator(IChangePasswordCommand? changePasswordCommand = null, UserManager<UserEntity>? userManager = null)
        {
            ILogger<ChangePasswordPostOrchestrator> logger = Substitute.For<ILogger<ChangePasswordPostOrchestrator>>();
            changePasswordCommand ??= Substitute.For<IChangePasswordCommand>();
            userManager ??= new UserManagerBuilder().Build();
            return new ChangePasswordPostOrchestrator(logger, changePasswordCommand, userManager);
        }


    }
}
