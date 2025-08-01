using Microsoft.Extensions.Logging;
using NSubstitute;
using SftpScheduler.Common;
using SftpScheduler.Common.IO;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Config;
using SftpSchedulerService.Workers;

namespace SftpSchedulerService.Tests.Workers
{
	[TestFixture]
	public class TempDataCleanupWorkerServiceTests
	{

		[Test]
		public void ExecuteCleanup_UpdaterFolderDoesNotExist_ExitsAndReturnsTrue()
		{
			// setup
			IDirectoryUtility dirUtility = new SubstituteBuilder<IDirectoryUtility>().Build();
            dirUtility.Exists(Arg.Any<string>()).Returns(false);

			// execute
			var workerService = CreateWorkerService(dirUtility: dirUtility);
			bool result = workerService.ExecuteCleanup();

			// assert
			Assert.That(result, Is.True);
			dirUtility.DidNotReceive().GetFiles(Arg.Any<string>(), Arg.Any<SearchOption>(), Arg.Any<string>());
		}

		[Test]
		public void ExecuteCleanup_NoFilesExist_ExitsAndReturnsTrue()
		{
			// setup
			string tempDataDirectory = "C:\\Temp\\Fake";

			AppSettings appSettings = new SubstituteBuilder<AppSettings>().Build();
			appSettings.TempDataDirectory.Returns(tempDataDirectory);
            IDirectoryUtility dirUtility = new SubstituteBuilder<IDirectoryUtility>().Build();
			dirUtility.Exists(tempDataDirectory).Returns(true);
			dirUtility.GetFiles(tempDataDirectory, SearchOption.AllDirectories).Returns(Array.Empty<string>());
				
			IFileUtility fileUtility = new SubstituteBuilder<IFileUtility>().Build();

			// execute
			var workerService = CreateWorkerService(appSettings: appSettings, dirUtility: dirUtility, fileUtility: fileUtility);
			bool result = workerService.ExecuteCleanup();

			// assert
			Assert.That(result, Is.True);
			dirUtility.Received(1).GetFiles(tempDataDirectory, SearchOption.AllDirectories);
			fileUtility.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>());
		}

		[Test]
		public void ExecuteCleanup_FilesExist_FilesAreDeleted()
		{
			// setup
			string[] files = { "C:\\Temp\\myfile1.txt", "C:\\Temp\\myfile2.pdf", "C:\\Temp\\myfile1.dll" };
			string[] renamedFiles = files.Select(x => x.Replace(".new", "")).ToArray();

            IDirectoryUtility dirUtility = new SubstituteBuilder<IDirectoryUtility>().Build();
            dirUtility.Exists(Arg.Any<string>()).Returns(true);
            dirUtility.GetFiles(Arg.Any<string>(), Arg.Any<SearchOption>(), Arg.Any<string>()).Returns(files);

			IFileUtility fileUtility = new SubstituteBuilder<IFileUtility>().Build();

			// execute
			var workerService = CreateWorkerService(dirUtility: dirUtility, fileUtility: fileUtility);
			bool result = workerService.ExecuteCleanup();

			// assert
			Assert.That(result, Is.True);

			for (int i=0; i<files.Length; i++)
			{
				fileUtility.Received(1).Delete(files[0]);
			}
		}

        [Test]
        public void ExecuteCleanup_FilesExistButFailsToDelete_ReturnsFalseForRetry()
        {
            // setup
            string[] files = { "C:\\Temp\\myfile1.txt" };
            string[] renamedFiles = files.Select(x => x.Replace(".new", "")).ToArray();

            IDirectoryUtility dirUtility = new SubstituteBuilder<IDirectoryUtility>().Build();
            dirUtility.Exists(Arg.Any<string>()).Returns(true);
            dirUtility.GetFiles(Arg.Any<string>(), Arg.Any<SearchOption>(), Arg.Any<string>()).Returns(files);

            IFileUtility fileUtility = new SubstituteBuilder<IFileUtility>().Build();
            fileUtility.When(x => x.Delete(Arg.Any<string>())).Throw(new Exception("test"));

            // execute
            var workerService = CreateWorkerService(dirUtility: dirUtility, fileUtility: fileUtility);
            bool result = workerService.ExecuteCleanup();

            // assert
            Assert.That(result, Is.False);
            fileUtility.Received(1).Delete(files[0]);
        }

        private TempDataCleanupWorkerService CreateWorkerService(AppSettings? appSettings = null, IDirectoryUtility? dirUtility = null, IFileUtility? fileUtility = null)
		{
			ILogger<TempDataCleanupWorkerService> logger = Substitute.For<ILogger<TempDataCleanupWorkerService>>();
			appSettings ??= Substitute.For<AppSettings>();
			dirUtility ??= Substitute.For<IDirectoryUtility>();
			fileUtility ??= Substitute.For<IFileUtility>();
			return new TempDataCleanupWorkerService(logger, appSettings, dirUtility, fileUtility);
		}
	}
}
