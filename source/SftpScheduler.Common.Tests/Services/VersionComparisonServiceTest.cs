using NSubstitute;
using NUnit.Framework;
using SftpScheduler.Common.Models;
using SftpScheduler.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.Common.Tests.Services
{
    [TestFixture]
    public class VersionComparisonServiceTest
    {
        private IApplicationVersionService _appVersionService;
        private IGitHubVersionService _gitHubVersionService;

        [SetUp]
        public void SetUp_VersionComparisonServiceTest()
        {
            _appVersionService = Substitute.For<IApplicationVersionService>();
            _gitHubVersionService = Substitute.For<IGitHubVersionService>();
    }

    [Test]
        public void CheckIfNewVersionAvailable_VersionsMatch_SetsValuesCorrectly()
        {
            string appFolder = Assembly.GetExecutingAssembly().Location;    
            string latestVersionUrl = Guid.NewGuid().ToString();
            const string version = "2.1.2";

            ApplicationVersionInfo appVersionInfo = new ApplicationVersionInfo();
            appVersionInfo.VersionNumber = version;

            _appVersionService.GetVersion(Arg.Any<string>()).Returns(version);
            _gitHubVersionService.GetVersionInfo(latestVersionUrl).Returns(Task.FromResult(appVersionInfo));

            VersionComparisonService versionComparisonService = new VersionComparisonService(_appVersionService, _gitHubVersionService);
            VersionComparisonResult result = versionComparisonService.CheckIfNewVersionAvailable(latestVersionUrl, appFolder).GetAwaiter().GetResult();

            _gitHubVersionService.Received(1).GetVersionInfo(latestVersionUrl);
            Assert.IsFalse(result.IsNewVersionAvailable);
            Assert.AreEqual(version, result.LatestReleaseVersionInfo!.VersionNumber);
        }

        [Test]
        public void CheckIfNewVersionAvailable_VersionsDoNotMatch_SetsValuesCorrectly()
        {
            string appFolder = Assembly.GetExecutingAssembly().Location;
            string latestVersionUrl = Guid.NewGuid().ToString();
            const string versionInstalled = "2.1.1";
            const string versionLatest = "2.1.3";

            ApplicationVersionInfo appVersionInfo = new ApplicationVersionInfo();
            appVersionInfo.VersionNumber = versionLatest;

            _appVersionService.GetVersion(appFolder).Returns(versionInstalled);
            _gitHubVersionService.GetVersionInfo(latestVersionUrl).Returns(Task.FromResult(appVersionInfo));

            VersionComparisonService versionComparisonService = new VersionComparisonService(_appVersionService, _gitHubVersionService);
            VersionComparisonResult result = versionComparisonService.CheckIfNewVersionAvailable(latestVersionUrl, appFolder).GetAwaiter().GetResult();

            _gitHubVersionService.Received(1).GetVersionInfo(latestVersionUrl);
            Assert.IsTrue(result.IsNewVersionAvailable);
            Assert.AreEqual(versionLatest, result.LatestReleaseVersionInfo!.VersionNumber);
        }

    }
}
