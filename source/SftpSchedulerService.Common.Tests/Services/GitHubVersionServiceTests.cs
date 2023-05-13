using NSubstitute;
using NUnit.Framework;
using SftpSchedulerService.Common.Services;
using SftpSchedulerService.Common.Tests.MockUtils.Web;
using SftpSchedulerService.Common.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Common.Tests.Services
{
    [TestFixture]
    public class GitHubVersionServiceTests
    {

        private const string GitHubLatestReleaseUrl = "https://api.github.com/repos/mrsalmon1976/SftpScheduler/releases/latest";

        [Test]
        public void GetVersionInfo_OnRequest_BindResponseValuesCorrectly()
        {
            // setup
            Web.IHttpClientFactory httpClientFactory = Substitute.For<Web.IHttpClientFactory>();

            string response = GetSampleGitHubReleaseJson();
            MockHttpMessageHandler httpMessageHandler = new MockHttpMessageHandler(HttpStatusCode.OK, response);
            HttpClient client = new HttpClient(httpMessageHandler);
            httpClientFactory.GetHttpClient().Returns(client);


            // execute
            IGitHubVersionService gitHubVersionService = new GitHubVersionService(httpClientFactory);
            var result = gitHubVersionService.GetVersionInfo(GitHubLatestReleaseUrl).GetAwaiter().GetResult();

            // assert
            Assert.AreEqual("0.0.1", result.VersionNumber);
        }

        [Test]
        public void GetVersionInfo_Integration_GetsLatestReleaseDataFromGitHub()
        {
            IGitHubVersionService gitHubVersionService = new GitHubVersionService(new HttpClientFactory());
            var result = gitHubVersionService.GetVersionInfo(GitHubLatestReleaseUrl).GetAwaiter().GetResult();
            string versionNumber = result.VersionNumber;
            System.Version version = System.Version.Parse(versionNumber);
            Assert.GreaterOrEqual(version.Major, 0);
        }

        private string GetSampleGitHubReleaseJson()
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream("SftpSchedulerService.Common.Tests.Resources.GitHubLatestVersionSample.json"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
