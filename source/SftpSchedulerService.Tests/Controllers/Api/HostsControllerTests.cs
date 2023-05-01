using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Quartz;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.ViewOrchestrators.Api.Cron;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SftpSchedulerService.Tests.Controllers.Api
{
    [TestFixture]
    public class HostsControllerTests
    {

        const string UrlGet = "/api/hosts";

        const string UrlGetById = "/api/hosts?id={0}";

        #region Get Tests

        [Test]
        public void  Get_ExecuteSuccess()
        {
            // setup
            string[] roles = { UserRoles.Admin };
            IHostFetchAllOrchestrator orchestrator = Substitute.For<IHostFetchAllOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostFetchAllOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteSuccess(UrlGet, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute();
        }

        [Test]
        public void Get_ExecuteUnauthorised()
        {
            // setup
            IHostFetchAllOrchestrator orchestrator = Substitute.For<IHostFetchAllOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostFetchAllOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteUnauthorised(UrlGet, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute();
        }

        [TestCase(UserRoles.Admin, UserRoles.User)]
        public void Get_CheckAllRoles(params string[] authorisedRoles)
        {
            IHostFetchAllOrchestrator orchestrator = Substitute.For<IHostFetchAllOrchestrator>();
            ControllerTestHelper.CheckAllRoles<IHostFetchAllOrchestrator>(orchestrator, UrlGet, authorisedRoles);
        }

        #endregion

    }
}
