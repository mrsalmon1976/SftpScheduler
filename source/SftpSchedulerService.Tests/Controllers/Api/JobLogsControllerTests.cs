using NSubstitute;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.Utilities;
using SftpSchedulerService.ViewOrchestrators.Api.JobLog;

namespace SftpSchedulerService.Tests.Controllers.Api
{
    [TestFixture]
    public class JobLogsControllerTests
    {
        const string UrlGetLogsByJobId = "/api/jobs/{0}/logs?maxLogId={1}";

        #region GetLogsByJobId Tests

        [Test]
        public void GetLogsByJobId_ExecuteSuccess()
        {
            // setup
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            int maxLogId = Faker.RandomNumber.Next(50, 100);
            string url = String.Format(UrlGetLogsByJobId, hashId, maxLogId);

            string[] roles = { UserRoles.Admin };
            IJobLogFetchAllOrchestrator orchestrator = Substitute.For<IJobLogFetchAllOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobLogFetchAllOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteSuccess(url, HttpMethod.Get, null, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute(hashId, maxLogId);
        }

        [Test]
        public void GetLogsByJobId_ExecuteUnauthorised()
        {
            // setup
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            int maxLogId = Faker.RandomNumber.Next(50, 100);
            string url = String.Format(UrlGetLogsByJobId, hashId, maxLogId);

            IJobLogFetchAllOrchestrator orchestrator = Substitute.For<IJobLogFetchAllOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobLogFetchAllOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteUnauthorised(url, HttpMethod.Get, null, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute(Arg.Any<string>(), Arg.Any<int>());
        }

        [TestCase(UserRoles.Admin, UserRoles.User)]
        public void GetLogsByJobId_CheckAllRoles(params string[] authorisedRoles)
        {
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            int maxLogId = Faker.RandomNumber.Next(50, 100);
            string url = String.Format(UrlGetLogsByJobId, hashId, maxLogId);

            IJobLogFetchAllOrchestrator orchestrator = Substitute.For<IJobLogFetchAllOrchestrator>();
            ControllerTestHelper.CheckAllRoles<IJobLogFetchAllOrchestrator>(orchestrator, url, HttpMethod.Get, null, authorisedRoles);
        }

        #endregion
    }
}
