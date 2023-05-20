using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.BootStrapping;
using SftpSchedulerService.Caching;
using SftpSchedulerService.Common.Models;
using SftpSchedulerService.Common.Services;
using SftpSchedulerService.Config;
using SftpSchedulerService.Models.Job;
using SftpSchedulerService.Models.Notification;
using SftpSchedulerService.Models.Update;
using SftpSchedulerService.ViewOrchestrators.Api.Job;
using SftpSchedulerService.ViewOrchestrators.Api.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Update
{
    [TestFixture]
    public class UpdateCheckOrchestratorTests
    {
        [Test]
        public void Execute_VersionCheckCached_ReturnsFromCache()
        {
            // setup
            string version = Faker.RandomNumber.Next(10, 20).ToString();
            VersionCheckViewModel versionCheckViewModel = new VersionCheckViewModel();

            versionCheckViewModel.LatestReleaseVersionNumber = version;
            ICacheProvider cacheProvider = Substitute.For<ICacheProvider>();
            cacheProvider.Get<VersionCheckViewModel>(Arg.Any<string>()).Returns(versionCheckViewModel);
            
            // execute
            IUpdateCheckOrchestrator orchestrator = CreateOrchestrator(cacheProvider: cacheProvider);
            var result = orchestrator.Execute().Result as OkObjectResult;

            // assert
            Assert.That(result, Is.Not.Null);

            VersionCheckViewModel resultViewModel = result.Value as VersionCheckViewModel;
            Assert.That(resultViewModel, Is.Not.Null);
            Assert.That(resultViewModel.LatestReleaseVersionNumber, Is.EqualTo(version));

            cacheProvider.DidNotReceive().Set(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<TimeSpan>());
        }

        [Test]
        public void Execute_VersionCheckNotCached_PerformsComparison()
        {
            // setup
            string version = Faker.RandomNumber.Next(10, 20).ToString();

            IVersionComparisonService versionComparisonService = Substitute.For<IVersionComparisonService>();
            AppSettings appSettings = Substitute.For<AppSettings>();
            appSettings.LatestVersionUrl.Returns("https://test.sftpscheduler/latestversion");
            appSettings.BaseDirectory.Returns(AppDomain.CurrentDomain.BaseDirectory);

            VersionComparisonResult versionComparisonResult = new VersionComparisonResult();
            versionComparisonResult.LatestReleaseVersionInfo = new ApplicationVersionInfo() { VersionNumber = version };
            versionComparisonService.CheckIfNewVersionAvailable(appSettings.LatestVersionUrl, appSettings.BaseDirectory).Returns(versionComparisonResult);

            // execute
            IUpdateCheckOrchestrator orchestrator = CreateOrchestrator(versionComparisonService, null, appSettings);
            var result = orchestrator.Execute().Result as OkObjectResult;

            // assert
            Assert.That(result, Is.Not.Null);

            VersionCheckViewModel resultViewModel = result.Value as VersionCheckViewModel;
            Assert.That(resultViewModel, Is.Not.Null);
            Assert.That(resultViewModel.LatestReleaseVersionNumber, Is.EqualTo(version));

            versionComparisonService.Received(1).CheckIfNewVersionAvailable(appSettings.LatestVersionUrl, appSettings.BaseDirectory);
        }

        [Test]
        public void Execute_VersionCheckNotCached_AddsToCache()
        {
            // setup
            string version = Faker.RandomNumber.Next(10, 20).ToString();
            VersionCheckViewModel? versionCheckViewModel = null;

            ICacheProvider cacheProvider = Substitute.For<ICacheProvider>();
            cacheProvider.Get<VersionCheckViewModel>(CacheKeys.VersionUpdateCheck).Returns(versionCheckViewModel);

            IVersionComparisonService versionComparisonService = Substitute.For<IVersionComparisonService>();
            VersionComparisonResult versionComparisonResult = new VersionComparisonResult();
            versionComparisonResult.LatestReleaseVersionInfo = new ApplicationVersionInfo() { VersionNumber = version };
            versionComparisonService.CheckIfNewVersionAvailable(Arg.Any<string>(), Arg.Any<string>()).Returns(versionComparisonResult);


            // execute
            IUpdateCheckOrchestrator orchestrator = CreateOrchestrator(versionComparisonService, cacheProvider);
            var result = orchestrator.Execute().Result as OkObjectResult;

            // assert
            Assert.That(result, Is.Not.Null);
            cacheProvider.Received(1).Set(CacheKeys.VersionUpdateCheck, Arg.Any<object>(), Arg.Any<TimeSpan>());

        }


        private UpdateCheckOrchestrator CreateOrchestrator(IVersionComparisonService? versionComparisonService = null, ICacheProvider? cacheProvider = null, AppSettings? appSettings = null)
        {
            versionComparisonService = (versionComparisonService ?? Substitute.For<IVersionComparisonService>());
            cacheProvider = (cacheProvider ?? Substitute.For<ICacheProvider>());
            appSettings = (appSettings ?? Substitute.For<AppSettings>());

            return new UpdateCheckOrchestrator(versionComparisonService, cacheProvider, appSettings);
        }

    }
}
