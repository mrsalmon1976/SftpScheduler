using NSubstitute;
using SftpScheduler.BLL.Identity;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.Utilities;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SftpSchedulerService.Tests.Controllers.Api
{
    [TestFixture]
    public class JobsControllerTests
    {
        const string UrlGet = "/api/jobs";

        const string UrlGetById = "/api/jobs/{0}";

        const string UrlPost = "/api/jobs";

        const string UrlPostUpdate = "/api/jobs/{0}";

        const string UrlPostRun = "/api/jobs/{0}/run";

        const string UrlDelete = "/api/jobs/{0}";

        #region Get Tests

        [Test]
        public void Get_ExecuteSuccess()
        {
            // setup
            string[] roles = { UserRoles.Admin };
            IJobFetchAllOrchestrator orchestrator = Substitute.For<IJobFetchAllOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobFetchAllOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteSuccess(UrlGet, HttpMethod.Get, null, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute();
        }

        [Test]
        public void GetLogsByJobId_ExecuteUnauthorised()
        {
            // setup

            IJobFetchAllOrchestrator orchestrator = Substitute.For<IJobFetchAllOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobFetchAllOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteUnauthorised(UrlGet, HttpMethod.Get, null, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute();
        }

        [TestCase(UserRoles.Admin, UserRoles.User)]
        public void GetLogsByJobId_CheckAllRoles(params string[] authorisedRoles)
        {
            IJobFetchAllOrchestrator orchestrator = Substitute.For<IJobFetchAllOrchestrator>();
            ControllerTestHelper.CheckAllRoles<IJobFetchAllOrchestrator>(orchestrator, UrlGet, HttpMethod.Get, null, authorisedRoles);
        }

        #endregion

        #region GetById Tests

        [Test]
        public void GetById_ExecuteSuccess()
        {
            // setup
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            string url = String.Format(UrlGetById, hashId);
            string[] roles = { UserRoles.Admin };
            IJobFetchOneOrchestrator orchestrator = Substitute.For<IJobFetchOneOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobFetchOneOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteSuccess(url, HttpMethod.Get, null, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute(hashId);
        }

        [Test]
        public void GetById_ExecuteUnauthorised()
        {
            // setup
            string hashId = UrlUtils.Encode(1);
            string url = String.Format(UrlGetById, hashId);
            IJobFetchOneOrchestrator orchestrator = Substitute.For<IJobFetchOneOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobFetchOneOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteUnauthorised(url, HttpMethod.Get, null, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute(Arg.Any<string>());
        }

        [TestCase(UserRoles.Admin, UserRoles.User)]
        public void GetById_CheckAllRoles(params string[] authorisedRoles)
        {
            string hashId = UrlUtils.Encode(1);
            string url = String.Format(UrlGetById, hashId);
            IJobFetchOneOrchestrator orchestrator = Substitute.For<IJobFetchOneOrchestrator>();
            ControllerTestHelper.CheckAllRoles<IJobFetchOneOrchestrator>(orchestrator, url, HttpMethod.Get, null, authorisedRoles);
        }

        #endregion

        #region Post Tests

        [Test]
        public void Post_ExecuteSuccess()
        {
            // setup
            string[] roles = { UserRoles.Admin };
            IJobCreateOrchestrator orchestrator = Substitute.For<IJobCreateOrchestrator>();
            JobViewModel jobViewModel = ViewModelTestHelper.CreateJobViewModel();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobCreateOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteSuccess(UrlPost, HttpMethod.Post, jobViewModel, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute(Arg.Any<JobViewModel>());
        }

        [Test]
        public void Post_ExecuteUnauthorised()
        {
            // setup
            IJobCreateOrchestrator orchestrator = Substitute.For<IJobCreateOrchestrator>();
            JobViewModel jobViewModel = ViewModelTestHelper.CreateJobViewModel();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobCreateOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteUnauthorised(UrlPost, HttpMethod.Post, jobViewModel, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute(Arg.Any<JobViewModel>());
        }

        [TestCase(UserRoles.Admin)]
        public void Post_CheckAllRoles(params string[] authorisedRoles)
        {
            IJobCreateOrchestrator orchestrator = Substitute.For<IJobCreateOrchestrator>();
            JobViewModel jobViewModel = ViewModelTestHelper.CreateJobViewModel();
            ControllerTestHelper.CheckAllRoles<IJobCreateOrchestrator>(orchestrator, UrlPost, HttpMethod.Post, jobViewModel, authorisedRoles);
        }

        #endregion

        #region PostUpdate Tests

        [Test]
        public void PostUpdate_ExecuteSuccess()
        {
            // setup
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            string url = String.Format(UrlPostUpdate, hashId);

            string[] roles = { UserRoles.Admin };
            IJobUpdateOrchestrator orchestrator = Substitute.For<IJobUpdateOrchestrator>();
            JobViewModel jobViewModel = ViewModelTestHelper.CreateJobViewModel();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobUpdateOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteSuccess(url, HttpMethod.Post, jobViewModel, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute(Arg.Any<JobViewModel>());
        }

        [Test]
        public void PostUpdate_ExecuteUnauthorised()
        {
            // setup
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            string url = String.Format(UrlPostUpdate, hashId);

            IJobUpdateOrchestrator orchestrator = Substitute.For<IJobUpdateOrchestrator>();
            HostViewModel hostViewModel = ViewModelTestHelper.CreateHostViewModel();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobUpdateOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteUnauthorised(url, HttpMethod.Get, hostViewModel, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute(Arg.Any<JobViewModel>());
        }

        [TestCase(UserRoles.Admin)]
        public void PostUpdate_CheckAllRoles(params string[] authorisedRoles)
        {
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            string url = String.Format(UrlPostUpdate, hashId);

            IJobUpdateOrchestrator orchestrator = Substitute.For<IJobUpdateOrchestrator>();
            HostViewModel hostViewModel = ViewModelTestHelper.CreateHostViewModel();
            ControllerTestHelper.CheckAllRoles<IJobUpdateOrchestrator>(orchestrator, url, HttpMethod.Post, hostViewModel, authorisedRoles);
        }

        #endregion

        #region PostRun Tests

        [Test]
        public void PostRun_ExecuteSuccess()
        {
            // setup
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            string url = String.Format(UrlPostRun, hashId);

            string[] roles = { UserRoles.Admin };
            IJobExecuteOrchestrator orchestrator = Substitute.For<IJobExecuteOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobExecuteOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteSuccess(url, HttpMethod.Post, null, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute(hashId);
        }

        [Test]
        public void PostRun_ExecuteUnauthorised()
        {
            // setup
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            string url = String.Format(UrlPostRun, hashId);

            IJobExecuteOrchestrator orchestrator = Substitute.For<IJobExecuteOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobExecuteOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteUnauthorised(url, HttpMethod.Post, null, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute(Arg.Any<string>());
        }

        [TestCase(UserRoles.Admin)]
        public void PostRun_CheckAllRoles(params string[] authorisedRoles)
        {
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            string url = String.Format(UrlPostRun, hashId);

            IJobExecuteOrchestrator orchestrator = Substitute.For<IJobExecuteOrchestrator>();
            ControllerTestHelper.CheckAllRoles<IJobExecuteOrchestrator>(orchestrator, url, HttpMethod.Post, null, authorisedRoles);
        }

        #endregion

        #region Delete Tests

        [Test]
        public void Delete_ExecuteSuccess()
        {
            // setup
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            string url = String.Format(UrlDelete, hashId);

            string[] roles = { UserRoles.Admin };
            IJobDeleteOneOrchestrator orchestrator = Substitute.For<IJobDeleteOneOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobDeleteOneOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteSuccess(url, HttpMethod.Delete, null, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute(hashId);
        }

        [Test]
        public void UrlDelete_ExecuteUnauthorised()
        {
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            string url = String.Format(UrlDelete, hashId);

            // setup
            IJobDeleteOneOrchestrator orchestrator = Substitute.For<IJobDeleteOneOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IJobDeleteOneOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteUnauthorised(url, HttpMethod.Delete, null, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute(Arg.Any<string>());
        }

        [TestCase(UserRoles.Admin)]
        public void Delete_CheckAllRoles(params string[] authorisedRoles)
        {
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            string url = String.Format(UrlDelete, hashId);

            IJobDeleteOneOrchestrator orchestrator = Substitute.For<IJobDeleteOneOrchestrator>();
            ControllerTestHelper.CheckAllRoles<IJobDeleteOneOrchestrator>(orchestrator, url, HttpMethod.Delete, null, authorisedRoles);
        }

        #endregion
    }
}
