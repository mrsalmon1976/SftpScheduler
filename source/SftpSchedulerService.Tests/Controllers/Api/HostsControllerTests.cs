﻿using NSubstitute;
using SftpScheduler.BLL.Identity;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.Utilities;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using SftpSchedulerService.ViewOrchestrators.Api.HostAuditLog;

namespace SftpSchedulerService.Tests.Controllers.Api
{
    [TestFixture]
    public class HostsControllerTests
    {

        const string UrlGet = "/api/hosts";

        const string UrlGetById = "/api/hosts/{0}";

        const string UrlPost = "/api/hosts";

        const string UrlPostUpdate = "/api/hosts/{0}";

        const string UrlPostScanFingerPrint = "/api/hosts/scanfingerprint";

        const string UrlDelete = "/api/hosts/{0}";

        const string UrlAuditLogs = "/api/hosts/{0}/auditlogs";

        #region Get Tests

        [Test]
        public void  Get_ExecuteSuccess()
        {
            // setup
            string[] roles = { UserRoles.Admin };
            IHostFetchAllOrchestrator orchestrator = Substitute.For<IHostFetchAllOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostFetchAllOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteSuccess(UrlGet, HttpMethod.Get, null, roles, configureServices);

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
            ControllerTestHelper.ExecuteUnauthorised(UrlGet, HttpMethod.Get, null, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute();
        }

        [TestCase(UserRoles.Admin, UserRoles.User)]
        public void Get_CheckAllRoles(params string[] authorisedRoles)
        {
            IHostFetchAllOrchestrator orchestrator = Substitute.For<IHostFetchAllOrchestrator>();
            ControllerTestHelper.CheckAllRoles<IHostFetchAllOrchestrator>(orchestrator, UrlGet, HttpMethod.Get, null, authorisedRoles);
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
            IHostFetchOneOrchestrator orchestrator = Substitute.For<IHostFetchOneOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostFetchOneOrchestrator>(orchestrator);

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
            IHostFetchOneOrchestrator orchestrator = Substitute.For<IHostFetchOneOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostFetchOneOrchestrator>(orchestrator);

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
            IHostFetchOneOrchestrator orchestrator = Substitute.For<IHostFetchOneOrchestrator>();
            ControllerTestHelper.CheckAllRoles<IHostFetchOneOrchestrator>(orchestrator, url, HttpMethod.Get, null, authorisedRoles);
        }

        #endregion

        #region Post Tests

        [Test]
        public void Post_ExecuteSuccess()
        {
            // setup
            string[] roles = { UserRoles.Admin };
            IHostCreateOrchestrator orchestrator = Substitute.For<IHostCreateOrchestrator>();
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostCreateOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteSuccess(UrlPost, HttpMethod.Post, hostViewModel, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute(Arg.Any<HostViewModel>(), Arg.Any<string>());
        }

        [Test]
        public void Post_ExecuteUnauthorised()
        {
            // setup
            IHostCreateOrchestrator orchestrator = Substitute.For<IHostCreateOrchestrator>();
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostCreateOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteUnauthorised(UrlPost, HttpMethod.Post, hostViewModel, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute(Arg.Any<HostViewModel>(), Arg.Any<string>());
        }

        [TestCase(UserRoles.Admin)]
        public void Post_CheckAllRoles(params string[] authorisedRoles)
        {
            IHostCreateOrchestrator orchestrator = Substitute.For<IHostCreateOrchestrator>();
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            ControllerTestHelper.CheckAllRoles<IHostCreateOrchestrator>(orchestrator, UrlPost, HttpMethod.Post, hostViewModel, authorisedRoles);
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
            IHostUpdateOrchestrator orchestrator = Substitute.For<IHostUpdateOrchestrator>();
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostUpdateOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteSuccess(url, HttpMethod.Post, hostViewModel, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute(Arg.Any<HostViewModel>(), Arg.Any<string>());
        }

        [Test]
        public void PostUpdate_ExecuteUnauthorised()
        {
            // setup
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            string url = String.Format(UrlPostUpdate, hashId);

            IHostUpdateOrchestrator orchestrator = Substitute.For<IHostUpdateOrchestrator>();
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostUpdateOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteUnauthorised(url, HttpMethod.Get, hostViewModel, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute(Arg.Any<HostViewModel>(), Arg.Any<string>());
        }

        [TestCase(UserRoles.Admin)]
        public void PostUpdate_CheckAllRoles(params string[] authorisedRoles)
        {
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            string url = String.Format(UrlPostUpdate, hashId);

            IHostUpdateOrchestrator orchestrator = Substitute.For<IHostUpdateOrchestrator>();
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            ControllerTestHelper.CheckAllRoles<IHostUpdateOrchestrator>(orchestrator, url, HttpMethod.Post, hostViewModel, authorisedRoles);
        }

        #endregion

        #region PostScanFingerprint Tests

        [Test]
        public void PostScanFingerprint_ExecuteSuccess()
        {
            // setup
            string[] roles = { UserRoles.Admin };
            IHostFingerprintScanOrchestrator orchestrator = Substitute.For<IHostFingerprintScanOrchestrator>();
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostFingerprintScanOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteSuccess(UrlPostScanFingerPrint, HttpMethod.Post, hostViewModel, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute(Arg.Any<HostViewModel>());
        }

        [Test]
        public void PostScanFingerprint_ExecuteUnauthorised()
        {
            // setup
            IHostFingerprintScanOrchestrator orchestrator = Substitute.For<IHostFingerprintScanOrchestrator>();
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostFingerprintScanOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteUnauthorised(UrlPostScanFingerPrint, HttpMethod.Get, hostViewModel, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute(Arg.Any<HostViewModel>());
        }

        [TestCase(UserRoles.Admin)]
        public void PostScanFingerprint_CheckAllRoles(params string[] authorisedRoles)
        {
            IHostFingerprintScanOrchestrator orchestrator = Substitute.For<IHostFingerprintScanOrchestrator>();
            HostViewModel hostViewModel = new SubstituteBuilder<HostViewModel>().WithRandomProperties().Build();
            ControllerTestHelper.CheckAllRoles<IHostFingerprintScanOrchestrator>(orchestrator, UrlPostScanFingerPrint, HttpMethod.Post, hostViewModel, authorisedRoles);
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
            IHostDeleteOneOrchestrator orchestrator = Substitute.For<IHostDeleteOneOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostDeleteOneOrchestrator>(orchestrator);

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
            IHostDeleteOneOrchestrator orchestrator = Substitute.For<IHostDeleteOneOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostDeleteOneOrchestrator>(orchestrator);

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

            IHostDeleteOneOrchestrator orchestrator = Substitute.For<IHostDeleteOneOrchestrator>();
            ControllerTestHelper.CheckAllRoles<IHostDeleteOneOrchestrator>(orchestrator, url, HttpMethod.Delete, null, authorisedRoles);
        }

        #endregion

        #region GetAuditLogs Tests

        [Test]
        public void GetAuditLogs_ExecuteSuccess()
        {
            // setup
            string hashId = UrlUtils.Encode(Faker.RandomNumber.Next(1, 100));
            string url = String.Format(UrlAuditLogs, hashId);
            string[] roles = { UserRoles.User };
            IHostAuditLogFetchAllOrchestrator orchestrator = Substitute.For<IHostAuditLogFetchAllOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostAuditLogFetchAllOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteSuccess(url, HttpMethod.Get, null, roles, configureServices);

            // assert
            orchestrator.Received(1).Execute(hashId);
        }

        [Test]
        public void GetAuditLogs_ExecuteUnauthorised()
        {
            // setup
            string hashId = UrlUtils.Encode(1);
            string url = String.Format(UrlGetById, hashId);
            IHostAuditLogFetchAllOrchestrator orchestrator = Substitute.For<IHostAuditLogFetchAllOrchestrator>();
            var configureServices = ControllerTestHelper.CreateConfiguration<IHostAuditLogFetchAllOrchestrator>(orchestrator);

            // execute
            ControllerTestHelper.ExecuteUnauthorised(url, HttpMethod.Get, null, configureServices);

            // assert
            orchestrator.DidNotReceive().Execute(Arg.Any<string>());
        }

        [TestCase(UserRoles.Admin, UserRoles.User)]
        public void GetAuditLogs_CheckAllRoles(params string[] authorisedRoles)
        {
            string hashId = UrlUtils.Encode(1);
            string url = String.Format(UrlAuditLogs, hashId);
            IHostAuditLogFetchAllOrchestrator orchestrator = Substitute.For<IHostAuditLogFetchAllOrchestrator>();
            ControllerTestHelper.CheckAllRoles<IHostAuditLogFetchAllOrchestrator>(orchestrator, url, HttpMethod.Get, null, authorisedRoles);
        }

        #endregion
    }
}
