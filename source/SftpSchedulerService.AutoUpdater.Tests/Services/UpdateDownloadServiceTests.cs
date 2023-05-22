using NUnit.Framework;
using SftpSchedulerService.AutoUpdater.Services;
using SftpScheduler.Common.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.AutoUpdater.Tests.Services
{
    [TestFixture]
    public class UpdateDownloadServiceTest
    {
        [Test]
        public void DownloadFile_Integration_CheckDownloaded()
        {
            const string GitHubReadMeUri = "https://github.com/mrsalmon1976/SftpScheduler/blob/master/README.md";

            // setup
            string downloadPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "readme.md");
            DeleteFileIfExists(downloadPath);

            // execute
            IUpdateDownloadService updateDownloadService = new UpdateDownloadService(new HttpClientFactory());
            updateDownloadService.DownloadFile(GitHubReadMeUri, downloadPath).GetAwaiter().GetResult();

            // assert
            Assert.IsTrue(File.Exists(downloadPath));
            DeleteFileIfExists(downloadPath);
        }

        private void DeleteFileIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

        }
    }
}
