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
    public class CronControllerTests
    {
        const string DefaultCronSchedule = "0 0/1 * ? * * *";

        const string UrlGet = "/api/cron?schedule={0}";

        #region Get Tests

        [Test]
        public void Get_ExecuteSuccess()
        {
            // setup
            string[] roles = { UserRoles.Admin };
            ICronGetScheduleOrchestrator orchestrator = Substitute.For<ICronGetScheduleOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<ICronGetScheduleOrchestrator>(orchestrator);
            var url = CreateGetUrl(DefaultCronSchedule);

            // execute
            ControllerTestHelper.ExecuteSuccess(url, HttpMethod.Get, null, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute(DefaultCronSchedule);
        }

        [Test]
        public void Get_ExecuteUnauthorised()
        {
            // setup
            ICronGetScheduleOrchestrator orchestrator = Substitute.For<ICronGetScheduleOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<ICronGetScheduleOrchestrator>(orchestrator);
            var url = CreateGetUrl(DefaultCronSchedule);

            // execute
            ControllerTestHelper.ExecuteUnauthorised(url, HttpMethod.Get, null, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute(Arg.Any<String>());

        }

        [TestCase(UserRoles.Admin, UserRoles.User)]
        public void Get_CheckAllRoles(params string[] authorisedRoles)
        {
            var url = CreateGetUrl(DefaultCronSchedule);
            ICronGetScheduleOrchestrator orchestrator = Substitute.For<ICronGetScheduleOrchestrator>();
            ControllerTestHelper.CheckAllRoles<ICronGetScheduleOrchestrator>(orchestrator, url, HttpMethod.Get, null, authorisedRoles);
        }

        #endregion


        private static string CreateGetUrl(string schedule)
        {
            return String.Format(UrlGet, WebUtility.UrlEncode(schedule));
        }
    }
}
