using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.User;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Tests.Builders.Identity;
using SftpScheduler.BLL.Validators;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Models.User;
using SftpSchedulerService.ViewOrchestrators.Api.User;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.User
{
    [TestFixture]
    public class UserCreateOrchestratorTests
    {
        [Test]
        public void Execute_OnDataValidationException_ReturnsBadRequest()
        {
            ICreateUserCommand createUserCommand = Substitute.For<ICreateUserCommand>();
            UserViewModel userViewModel = new SubstituteBuilder<UserViewModel>().WithRandomProperties().Build();

            createUserCommand.ExecuteAsync(Arg.Any<UserManager<UserEntity>>(), Arg.Any<UserEntity>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>()).Throws(new DataValidationException("exception", new ValidationResult(new string[] { "error" })));

            IUserCreateOrchestrator userCreateOrchestrator = CreateOrchestrator(createUserCommand: createUserCommand);
            var result = userCreateOrchestrator.Execute(userViewModel).Result as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<ValidationResult>());

        }

        [Test]
        public void Execute_OnSave_ReturnsOk()
        {
            UserViewModel userViewModel = new SubstituteBuilder<UserViewModel>().WithRandomProperties().Build();

            IUserCreateOrchestrator userCreateOrchestrator = CreateOrchestrator();
            var result = userCreateOrchestrator.Execute(userViewModel).Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.InstanceOf<UserViewModel>());
        }

        [Test]
        public void Execute_OnSave_ReturnsResultWithId()
        {
            UserViewModel userViewModel = new SubstituteBuilder<UserViewModel>().WithRandomProperties().Build();
            string userId = Guid.NewGuid().ToString();
            UserEntity userEntity = new SubstituteBuilder<UserEntity>().WithRandomProperties().WithProperty(x => x.Id, userId).Build();

            ICreateUserCommand createUserCommand = Substitute.For<ICreateUserCommand>();
            createUserCommand.ExecuteAsync(Arg.Any<UserManager<UserEntity>>(), Arg.Any<UserEntity>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>()).Returns(userEntity);

            IUserCreateOrchestrator userCreateOrchestrator = CreateOrchestrator(createUserCommand: createUserCommand);
            var result = userCreateOrchestrator.Execute(userViewModel).Result as OkObjectResult;

            UserViewModel resultModel = result.Value as UserViewModel;
            Assert.That(resultModel, Is.Not.Null);
            Assert.That(resultModel.Id, Is.Not.EqualTo(userViewModel.Id));
            Assert.That(resultModel.Id, Is.EqualTo(userId));
        }

        private IUserCreateOrchestrator CreateOrchestrator(UserManager<UserEntity>? userManager = null, ICreateUserCommand? createUserCommand = null)
        {
            userManager ??= new UserManagerBuilder().Build();
            createUserCommand ??= Substitute.For<ICreateUserCommand>();

            return new UserCreateOrchestrator(userManager, createUserCommand);

        }

    }
}
