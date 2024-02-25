using Microsoft.Extensions.Logging;
using NSubstitute;
using SftpScheduler.Common;
using SftpScheduler.Common.IO;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Config;
using SftpSchedulerService.Tests.Builders.Config;
using SftpSchedulerService.Workers;

namespace SftpSchedulerService.Tests.Workers
{
	[TestFixture]
	public class UpdateCleanupWorkerServiceTests
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
			string baseDirectory = $"C:\\Temp\\{Guid.NewGuid()}";
			string updaterDirectory = Path.Combine(baseDirectory, UpdateConstants.UpdaterFolderName);
			string searchPattern = "*" + UpdateConstants.UpdaterNewFileExtension;

			AppSettings appSettings = new AppSettingsBuilder().WithBaseDirectory(baseDirectory).Build();
            IDirectoryUtility dirUtility = new SubstituteBuilder<IDirectoryUtility>().Build();
			dirUtility.Exists(updaterDirectory).Returns(true);
			dirUtility.GetFiles(updaterDirectory, SearchOption.AllDirectories, searchPattern).Returns(Array.Empty<string>());
				
			IFileUtility fileUtility = new SubstituteBuilder<IFileUtility>().Build();

			// execute
			var workerService = CreateWorkerService(appSettings: appSettings, dirUtility: dirUtility, fileUtility: fileUtility);
			bool result = workerService.ExecuteCleanup();

			// assert
			Assert.That(result, Is.True);
			dirUtility.Received(1).GetFiles(updaterDirectory, SearchOption.AllDirectories, searchPattern);
			fileUtility.DidNotReceive().Move(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>());
		}

		[Test]
		public void ExecuteCleanup_FilesExist_FilesAreRenamed()
		{
			// setup
			string[] files = { "C:\\Temp\\myfile1.txt.new", "C:\\Temp\\myfile2.pdf.new", "C:\\Temp\\myfile1.dll.new" };
			string[] renamedFiles = files.Select(x => x.Replace(".new", "")).ToArray();

            IDirectoryUtility dirUtility = new SubstituteBuilder<IDirectoryUtility>().Build();
            dirUtility.Exists(Arg.Any<string>()).Returns(true);
            dirUtility.GetFiles(Arg.Any<string>(), Arg.Any<SearchOption>(), Arg.Any<string>()).Returns(files);

			IFileUtility fileUtility = new SubstituteBuilder<IFileUtility>().Build();

			// execute
			var workerService = CreateWorkerService(dirUtility: dirUtility, fileUtility: fileUtility);
			bool result = workerService.ExecuteCleanup();

			// assert
			Assert.That(result, Is.False);

			for (int i=0; i<files.Length; i++)
			{
				fileUtility.Received(1).Move(files[0], renamedFiles[0], true);
			}
		}

		private UpdateCleanupWorkerService CreateWorkerService(AppSettings? appSettings = null, IDirectoryUtility? dirUtility = null, IFileUtility? fileUtility = null)
		{
			ILogger<UpdateCleanupWorkerService> logger = Substitute.For<ILogger<UpdateCleanupWorkerService>>();
			appSettings ??= Substitute.For<AppSettings>();
			dirUtility ??= Substitute.For<IDirectoryUtility>();
			fileUtility ??= Substitute.For<IFileUtility>();
			return new UpdateCleanupWorkerService(logger, appSettings, dirUtility, fileUtility);
		}
	}
}
