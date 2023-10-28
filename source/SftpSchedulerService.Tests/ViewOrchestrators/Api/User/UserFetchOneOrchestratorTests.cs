using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Tests.Builders.Models;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.Models.User;
using SftpSchedulerService.Utilities;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using SftpSchedulerService.ViewOrchestrators.Api.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.User
{
    [TestFixture]
    public class UserFetchOneOrchestratorTests
    {
        [Test]
        public void Execute_UserNotFound_Returns404()
        {
            // setup
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            UserEntity? userEntity = null;
            userManager.FindByIdAsync(Arg.Any<string>()).Returns(userEntity!);


            // execute
            IUserFetchOneOrchestrator orchestrator = CreateOrchestrator(userManager);
            var result = orchestrator.Execute(Guid.NewGuid().ToString()).GetAwaiter().GetResult() as NotFoundResult;

            // assert
            Assert.That(result, Is.Not.Null);

        }

        [Test]
        public void Execute_FetchesEntityAndReturnsAsViewModel()
        {
            // setup
            UserManager<UserEntity> userManager = IdentityTestHelper.CreateUserManagerMock();
            UserEntity userEntity = new UserEntityBuilder().WithRandomProperties().Build();
            userManager.FindByIdAsync(Arg.Any<string>()).Returns(userEntity);

            // execute
            IUserFetchOneOrchestrator orchestrator = CreateOrchestrator(userManager);
            var result = orchestrator.Execute(userEntity.Id).GetAwaiter().GetResult() as OkObjectResult;

            // assert
            Assert.That(result, Is.Not.Null);
            userManager.Received(1).FindByIdAsync(userEntity.Id);

            UserViewModel? userViewModel = result.Value as UserViewModel;
            Assert.That(userViewModel, Is.Not.Null);

        }

        private IUserFetchOneOrchestrator CreateOrchestrator(UserManager<UserEntity> userManager)
        {
            return new UserFetchOneOrchestrator(userManager);
        }

    }

}
