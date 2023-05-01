using NSubstitute;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.Utilities;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using SftpSchedulerService.ViewOrchestrators.Api.JobLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SftpSchedulerService.Tests.Controllers.Api
{
    [TestFixture]
    public class NotificationsLogsControllerTests
    {
        const string UrlGetJobNotifications = "/api/notifications/jobs?forceReload={0}";

        #region GetJobNotifications Tests

        [Test]
        public void GetJobNotifications_ExecuteSuccess()
        {
            // setup
            bool forceReload = (Faker.RandomNumber.Next(1, 100) < 50);
            string url = String.Format(UrlGetJobNotifications, forceReload);

            string[] roles = { UserRoles.Admin };
            IJobNotificationFetchAllOrchestrator orchestrator = Substitute.For<IJobNotificationFetchAllOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobNotificationFetchAllOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteSuccess(url, HttpMethod.Get, null, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute(forceReload);
        }

        [Test]
        public void GetJobNotifications_ExecuteUnauthorised()
        {
            // setup
            bool forceReload = (Faker.RandomNumber.Next(1, 100) < 50);
            string url = String.Format(UrlGetJobNotifications, forceReload);

            IJobNotificationFetchAllOrchestrator orchestrator = Substitute.For<IJobNotificationFetchAllOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobNotificationFetchAllOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteUnauthorised(url, HttpMethod.Get, null, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute(Arg.Any<bool>());
        }

        [TestCase(UserRoles.Admin, UserRoles.User)]
        public void GetJobNotifications_CheckAllRoles(params string[] authorisedRoles)
        {
            bool forceReload = (Faker.RandomNumber.Next(1, 100) < 50);
            string url = String.Format(UrlGetJobNotifications, forceReload);

            IJobNotificationFetchAllOrchestrator orchestrator = Substitute.For<IJobNotificationFetchAllOrchestrator>();
            ControllerTestHelper.CheckAllRoles<IJobNotificationFetchAllOrchestrator>(orchestrator, url, HttpMethod.Get, null, authorisedRoles);
        }

        #endregion
    }
}
