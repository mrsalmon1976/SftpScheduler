using NSubstitute;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.ViewOrchestrators.Api.Report;

namespace SftpSchedulerService.Tests.Controllers.Api
{
    [TestFixture]
    public class ReportsControllerTests
    {

        const string DateFormat = "yyyy-MM-dd";

        const string UrlGetNoTransfers = "/api/reports/notransfers";

        #region NoTransfers Tests

        [Test]
        public void NoTransfers_ExecuteSuccess()
        {
            // setup
            string[] roles = { UserRoles.User };
            IReportJobsWithNoTransfersOrchestrator orchestrator = Substitute.For<IReportJobsWithNoTransfersOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IReportJobsWithNoTransfersOrchestrator>(orchestrator);

            DateTime startDate = DateTime.Today.AddDays(-1);
            DateTime endDate = DateTime.Today;
            string url = $"{UrlGetNoTransfers}?startDate={startDate.ToString(DateFormat)}&endDate={endDate.ToString(DateFormat)}";

            // execute
            ControllerTestHelper.ExecuteSuccess(url, HttpMethod.Get, null, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute(startDate, endDate);
        }

        [Test]
        public void NoTransfers_ExecuteUnauthorised()
        {
            // setup
            IReportJobsWithNoTransfersOrchestrator orchestrator = Substitute.For<IReportJobsWithNoTransfersOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IReportJobsWithNoTransfersOrchestrator>(orchestrator);

            DateTime startDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;
            string url = $"{UrlGetNoTransfers}?startDate={startDate.ToString(DateFormat)}&endDate={endDate.ToString(DateFormat)}";

            // execute
            ControllerTestHelper.ExecuteUnauthorised(url, HttpMethod.Get, null, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute(Arg.Any<DateTime>(), Arg.Any<DateTime>());
        }

        [TestCase(UserRoles.Admin, UserRoles.User)]
        public void Get_CheckAllRoles(params string[] authorisedRoles)
        {
            IReportJobsWithNoTransfersOrchestrator orchestrator = Substitute.For<IReportJobsWithNoTransfersOrchestrator>();

            DateTime startDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;
            string url = $"{UrlGetNoTransfers}?startDate={startDate.ToString(DateFormat)}&endDate={endDate.ToString(DateFormat)}";

            ControllerTestHelper.CheckAllRoles<IReportJobsWithNoTransfersOrchestrator>(orchestrator, url, HttpMethod.Get, null, authorisedRoles);
        }

        #endregion

    }
}
