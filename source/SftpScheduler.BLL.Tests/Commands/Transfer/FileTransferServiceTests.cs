using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.Common.IO;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SftpScheduler.BLL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SftpScheduler.BLL.Commands.Job;

namespace SftpScheduler.BLL.Tests.Commands.Transfer
{
    [TestFixture]
    public class FileTransferServiceTests
    {
        #region DownloadFiles Test

        [Test]
        public void DownloadFiles_NoFiles_Exits()
        {
            // setup
            DownloadOptions options = CreateDownloadOptions();
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapper.ListDirectory(options.RemotePath).Returns(Enumerable.Empty<RemoteFileInfo>());
            IDbContext dbContext = Substitute.For<IDbContext>();

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            fileTransferService.DownloadFiles(sessionWrapper, dbContext, options);

            //assert
            sessionWrapper.Received(1).ListDirectory(options.RemotePath);
            sessionWrapper.DidNotReceive().GetFiles(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>());
        }

        [Test]
        public void DownloadFilesAvailable_DirectoriesOnRemote_OnlyFilesActioned()
        {
            // setup
            DownloadOptions options = CreateDownloadOptions();
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            int fileCount = Faker.RandomNumber.Next(2, 10);
            var remoteFiles = this.CreateRemoteFileList(fileCount, options.RemotePath, false);
            remoteFiles.AddRange(this.CreateRemoteFileList(2, options.RemotePath, true));
            sessionWrapper.ListDirectory(options.RemotePath).Returns(remoteFiles);
			IDbContext dbContext = Substitute.For<IDbContext>();

			// execute
			IFileTransferService fileTransferService = CreateFileTransferService();
            fileTransferService.DownloadFiles(sessionWrapper, dbContext, options);

            //assert
            sessionWrapper.Received(1).ListDirectory(options.RemotePath);
            sessionWrapper.Received(fileCount).GetFiles(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>());

        }


        [TestCase(true)]
        [TestCase(false)]
        public void DownloadFiles_FileDownload_ObeysDelete(bool deleteAfterDownload)
        {
            // setup
            DownloadOptions options = CreateDownloadOptions(deleteAfterDownload: deleteAfterDownload);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
			IDbContext dbContext = Substitute.For<IDbContext>();

			var remoteFiles = this.CreateRemoteFileList(1, options.RemotePath, false);
            sessionWrapper.ListDirectory(options.RemotePath).Returns(remoteFiles);
            string remoteFileName = remoteFiles[0].FullName;


            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            fileTransferService.DownloadFiles(sessionWrapper, dbContext, options);

            // assert
            sessionWrapper.Received(1).GetFiles(remoteFileName, Arg.Any<string>(), deleteAfterDownload);
        }

        [Test]
        public void DownloadFiles_FileDownloadWithDelete_NoMoveOccurs()
        {
            // setup
            DownloadOptions options = CreateDownloadOptions(deleteAfterDownload: true);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
			IDbContext dbContext = Substitute.For<IDbContext>();

			int fileCount = Faker.RandomNumber.Next(2, 7);
            var remoteFiles = this.CreateRemoteFileList(fileCount, options.RemotePath, false);
            sessionWrapper.ListDirectory(options.RemotePath).Returns(remoteFiles);

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            fileTransferService.DownloadFiles(sessionWrapper, dbContext, options);

            // assert
            sessionWrapper.Received(fileCount).GetFiles(Arg.Any<string>(), Arg.Any<string>(), true);
            sessionWrapper.DidNotReceive().MoveFile(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void DownloadFiles_FileDownloadWithArchive_MovesFileIntoArchiveFolder()
        {
            // setup
            string remoteArchivePath = $"/{Path.GetRandomFileName()}/";
            DownloadOptions options = CreateDownloadOptions(deleteAfterDownload: false, remoteArchivePath: remoteArchivePath);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
			IDbContext dbContext = Substitute.For<IDbContext>();

			int fileCount = Faker.RandomNumber.Next(2, 7);
            var remoteFiles = this.CreateRemoteFileList(fileCount, options.RemotePath, false);
            sessionWrapper.ListDirectory(options.RemotePath).Returns(remoteFiles);

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            fileTransferService.DownloadFiles(sessionWrapper, dbContext, options);

            // assert
            sessionWrapper.Received(fileCount).GetFiles(Arg.Any<string>(), Arg.Any<string>(), false);
            sessionWrapper.Received(fileCount).MoveFile(Arg.Any<string>(), Arg.Any<string>());

            foreach (RemoteFileInfo downloadFile in remoteFiles)
            {
                string targetPath = $"{options.RemoteArchivePath}{downloadFile.Name}";
                sessionWrapper.Received(1).MoveFile(downloadFile.FullName, targetPath);
            }
        }

        [Test]
        public void DownloadFiles_LocalCopiesSpecified_LocalCopiesMade()
        {
            // setup
            DownloadOptions options = CreateDownloadOptions();

            string localCopyFolder1 = $"C:\\Temp\\{Path.GetRandomFileName()}";
            string localCopyFolder2 = $"\\\\server\\Share\\{Path.GetRandomFileName()}";
            options.LocalCopyPaths.Add(localCopyFolder1);
            options.LocalCopyPaths.Add(localCopyFolder2);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
			IDbContext dbContext = Substitute.For<IDbContext>();

			var remoteFiles = this.CreateRemoteFileList(1, options.RemotePath, false);
            sessionWrapper.ListDirectory(options.RemotePath).Returns(remoteFiles);

            // execute
            IFileUtility fileWrap = Substitute.For<IFileUtility>();
            IFileTransferService fileTransferService = CreateFileTransferService(fileWrap: fileWrap);
            fileTransferService.DownloadFiles(sessionWrapper, dbContext, options);

            // assert

            foreach (string localCopyFolder in options.LocalCopyPaths)
            {
                string fileName = remoteFiles[0].Name;
                string sourcePath = Path.Combine(options.LocalPath, fileName);
                string targetPath = Path.Combine(localCopyFolder, fileName);
                fileWrap.Received(1).Copy(sourcePath, targetPath);
            }

        }

		[Test]
		public void DownloadFiles_FileDownload_ExecutesCreateJobFileLogCommand()
		{
            // setup
            DateTime testStartDate = DateTime.Now;
			int jobId = Faker.RandomNumber.Next(1, 1000);
			DownloadOptions options = CreateDownloadOptions(jobId);

			ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
			IDbContext dbContext = Substitute.For<IDbContext>();

			var remoteFiles = this.CreateRemoteFileList(1, options.RemotePath, false);
			sessionWrapper.ListDirectory(options.RemotePath).Returns(remoteFiles);

			int fileLength = Faker.RandomNumber.Next(1, 1000);
			IFileUtility fileWrap = Substitute.For<IFileUtility>();
			fileWrap.GetFileSize(Arg.Any<string>()).Returns(fileLength);

			// execute
			ICreateJobFileLogCommand createJobFileLogCmd = Substitute.For<ICreateJobFileLogCommand>();
			JobFileLogEntity? logEntity = null;
			createJobFileLogCmd.When(x => x.ExecuteAsync(dbContext, Arg.Any<JobFileLogEntity>())).Do(ci =>
			{
				logEntity = ci.ArgAt<JobFileLogEntity>(1);
			});

			IFileTransferService fileTransferService = CreateFileTransferService(createJobFileLogCommand: createJobFileLogCmd, fileWrap: fileWrap);
			fileTransferService.DownloadFiles(sessionWrapper, dbContext, options);

			// assert
            createJobFileLogCmd.Received(1).ExecuteAsync(dbContext, Arg.Any<JobFileLogEntity>());
			Assert.That(logEntity, Is.Not.Null);
			Assert.That(logEntity.JobId, Is.EqualTo(jobId));
			Assert.That(logEntity.FileName, Is.EqualTo(remoteFiles[0].Name));
			Assert.That(logEntity.FileLength, Is.EqualTo(fileLength));
			Assert.That(logEntity.StartDate, Is.GreaterThanOrEqualTo(testStartDate));
			Assert.That(logEntity.EndDate, Is.GreaterThanOrEqualTo(logEntity.StartDate));
		}


		#endregion

		#region UploadFilesAvailable Tests

		[Test]
        public void UploadFilesAvailable_NoNewFiles_NoFilesReturned()
        {
            int fileCount = Faker.RandomNumber.Next(2, 9);
            IEnumerable<string> files = CreateLocalFileList(fileCount, true);

            string localPath = AppDomain.CurrentDomain.BaseDirectory;

            IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
            directoryWrap.EnumerateFiles(localPath).Returns(files);

            IFileTransferService fileTransferService = CreateFileTransferService(directoryWrap: directoryWrap);
            IEnumerable<string> availableFiles = fileTransferService.UploadFilesAvailable(localPath);

            Assert.That(availableFiles.Count(), Is.EqualTo(0));
        }

        [Test]
        public void UploadFilesAvailable_AllNewFiles_AllFilesReturned()
        {
            int fileCount = Faker.RandomNumber.Next(2, 9);
            IEnumerable<string> files = CreateLocalFileList(fileCount, false);

            string localPath = AppDomain.CurrentDomain.BaseDirectory;

            IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
            directoryWrap.EnumerateFiles(localPath).Returns(files);

            IFileTransferService fileTransferService = CreateFileTransferService(directoryWrap: directoryWrap);
            IEnumerable<string> availableFiles = fileTransferService.UploadFilesAvailable(localPath);

            Assert.That(availableFiles.Count(), Is.EqualTo(fileCount));
        }


        [Test]
        public void UploadFilesAvailables_SomeNewFiles_SomeNewFilesReturned()
        {
            IEnumerable<string> uploadedFiles = CreateLocalFileList(Faker.RandomNumber.Next(2, 5), true);
            List<string> newFiles = CreateLocalFileList(Faker.RandomNumber.Next(6, 9), false);
            List<string> allFiles = new List<string>(uploadedFiles);
            allFiles.AddRange(newFiles);

            string localPath = AppDomain.CurrentDomain.BaseDirectory;

            IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
            directoryWrap.EnumerateFiles(localPath).Returns(allFiles);

            IFileTransferService fileTransferService = CreateFileTransferService(directoryWrap: directoryWrap);
            IEnumerable<string> availableFiles = fileTransferService.UploadFilesAvailable(localPath);

            Assert.That(newFiles.Count, Is.EqualTo(availableFiles.Count()));
            foreach (string file in newFiles)
            {
                Assert.That(availableFiles.Contains(file));
            }
        }

        #endregion

        #region UploadFiles Tests


        [Test]
        public void UploadFiles_FilesAvailableForUpload_MarkedAsUploaded()
        {
            List<string> newFiles = CreateLocalFileList(Faker.RandomNumber.Next(6, 9), false);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            string localPath = AppDomain.CurrentDomain.BaseDirectory;
            string remotePath = $"/{Path.GetRandomFileName()}/";

            IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
            directoryWrap.EnumerateFiles(localPath).Returns(newFiles);

			IDbContext dbContext = Substitute.For<IDbContext>();
			IFileUtility fileWrap = Substitute.For<IFileUtility>();

            IFileTransferService fileTransferService = CreateFileTransferService(directoryWrap: directoryWrap, fileWrap: fileWrap);

			UploadOptions options = new UploadOptions(1, newFiles, remotePath);
			fileTransferService.UploadFiles(sessionWrapper, dbContext, options);


			fileWrap.Received(newFiles.Count).Move(Arg.Any<string>(), Arg.Any<string>(), false);
            foreach (string file in newFiles)
            {
                fileWrap.Received(1).Move(file, Arg.Any<string>(), false);
            }
        }

        [Test]
        public void UploadFiles_FilesAvailableForUploadAndRenameExists_MarkedAsUploadedWithSuffix()
        {
            // setup
            List<string> newFiles = CreateLocalFileList(1, false);
            string localFileName = newFiles[0];
            string localFileExt = Path.GetExtension(localFileName);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            string localPath = AppDomain.CurrentDomain.BaseDirectory;
            string remotePath = $"/{Path.GetRandomFileName()}/";

			IDbContext dbContext = Substitute.For<IDbContext>();
			IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
            directoryWrap.EnumerateFiles(localPath).Returns(newFiles);

            IFileUtility fileWrap = Substitute.For<IFileUtility>();

            string expectedRename1 = localFileName.Replace(localFileExt, $".uploaded{localFileExt}");
            string expectedRename2 = localFileName.Replace(localFileExt, $".uploaded_1{localFileExt}");
            fileWrap.Exists(expectedRename1).Returns(true);

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(directoryWrap: directoryWrap, fileWrap: fileWrap);
			UploadOptions options = new UploadOptions(1, newFiles, remotePath);
			fileTransferService.UploadFiles(sessionWrapper, dbContext, options);


			// assert
			fileWrap.Received(1).Exists(expectedRename1);
            fileWrap.Received(1).Move(localFileName, expectedRename2, false);
        }

        [Test]
        public void UploadFiles_FileUpload_ExecutesCreateJobFileLogCommand()
        {
			// setup
			List<string> newFiles = CreateLocalFileList(1, false);
            int jobId = Faker.RandomNumber.Next(1, 1000);
			string remotePath = $"/{Path.GetRandomFileName()}/";
            DateTime testStartDate = DateTime.Now;

			IDbContext dbContext = Substitute.For<IDbContext>();
			IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
			directoryWrap.EnumerateFiles(Arg.Any<string>()).Returns(newFiles);

			int fileLength = Faker.RandomNumber.Next(1, 1000);
			IFileUtility fileWrap = Substitute.For<IFileUtility>();
            fileWrap.GetFileSize(Arg.Any<string>()).Returns(fileLength);

            ICreateJobFileLogCommand createJobFileLogCommand = Substitute.For<ICreateJobFileLogCommand>();
            JobFileLogEntity? logEntity = null;
			createJobFileLogCommand.When(x => x.ExecuteAsync(dbContext, Arg.Any<JobFileLogEntity>())).Do(ci =>
            {
                logEntity = ci.ArgAt<JobFileLogEntity>(1);
			});

			// execute
			IFileTransferService fileTransferService = CreateFileTransferService(directoryWrap: directoryWrap, fileWrap: fileWrap, createJobFileLogCommand: createJobFileLogCommand);
			UploadOptions options = new UploadOptions(jobId, newFiles, remotePath);
			fileTransferService.UploadFiles(Substitute.For<ISessionWrapper>(), dbContext, options);

            // assert
            Assert.That(logEntity, Is.Not.Null);
			Assert.That(logEntity.JobId, Is.EqualTo(jobId));
			Assert.That(logEntity.FileName, Is.EqualTo(Path.GetFileName(newFiles[0])));
			Assert.That(logEntity.FileLength, Is.EqualTo(fileLength));
			Assert.That(logEntity.StartDate, Is.GreaterThanOrEqualTo(testStartDate));
			Assert.That(logEntity.EndDate, Is.GreaterThanOrEqualTo(logEntity.StartDate));
		}


		[Test]
        public void UploadFiles_FilesAvailableForUpload_FilesUploaded()
        {
            List<string> newFiles = CreateLocalFileList(Faker.RandomNumber.Next(6, 9), false);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            string localPath = AppDomain.CurrentDomain.BaseDirectory;
            string remotePath = $"/{Path.GetRandomFileName()}/";
			IDbContext dbContext = Substitute.For<IDbContext>();

			IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
            directoryWrap.EnumerateFiles(localPath).Returns(newFiles);

            IFileUtility fileWrap = Substitute.For<IFileUtility>();

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(directoryWrap: directoryWrap, fileWrap: fileWrap);
            UploadOptions options = new UploadOptions(1, newFiles, remotePath);
			fileTransferService.UploadFiles(sessionWrapper, dbContext, options);

            // assert
            sessionWrapper.Received(newFiles.Count).PutFiles(Arg.Any<string>(), remotePath);
            foreach (string file in newFiles)
            {
                sessionWrapper.Received(1).PutFiles(file, remotePath);
            }
        }

		[Test]
		public void UploadFiles_FilesAvailableForUpload_JobFileLogCommandExecuted()
		{
			List<string> newFiles = CreateLocalFileList(Faker.RandomNumber.Next(6, 9), false);

			ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            IDbContext dbContext = Substitute.For<IDbContext>();    
			string localPath = AppDomain.CurrentDomain.BaseDirectory;
			string remotePath = $"/{Path.GetRandomFileName()}/";

			IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
			directoryWrap.EnumerateFiles(localPath).Returns(newFiles);

			ICreateJobFileLogCommand createJobFileCmd = Substitute.For<ICreateJobFileLogCommand>();

			// execute
			IFileTransferService fileTransferService = CreateFileTransferService(createJobFileLogCommand: createJobFileCmd, directoryWrap: directoryWrap);
			UploadOptions options = new UploadOptions(1, newFiles, remotePath);
			fileTransferService.UploadFiles(sessionWrapper, dbContext, options);

			// assert
			createJobFileCmd.Received(newFiles.Count).ExecuteAsync(dbContext, Arg.Any<JobFileLogEntity>());
		}
		#endregion

		#region Private Methods

		private DownloadOptions CreateDownloadOptions(int jobId = 0, string remotePath = "/Test/", string localPath = "C:\\Temp", bool deleteAfterDownload = false, string? remoteArchivePath = null)
        {
            return new DownloadOptions(jobId, localPath, remotePath)
            {
                DeleteAfterDownload = deleteAfterDownload,
                RemoteArchivePath = remoteArchivePath
            };
        }

        private List<string> CreateLocalFileList(int count, bool isUploaded)
        {
            List<string> files = new List<string>();
            string uploadedToken = (isUploaded ? ".uploaded" : "");
            for (int i=0; i<count; i++)
            {
                int fileNum = Faker.RandomNumber.Next(-10, 10);
                bool generateSuffix = isUploaded && fileNum <= 1;
                string nameSuffix = (generateSuffix ? "" : fileNum.ToString());

                string fileName = $"C:\\Temp\\file_{i}{uploadedToken}{nameSuffix}.zip";
                files.Add(fileName);
            }
            return files;
        }

        private List<SftpScheduler.BLL.Models.RemoteFileInfo> CreateRemoteFileList(int count, string remotePath, bool isDirectory)
        {
            List<SftpScheduler.BLL.Models.RemoteFileInfo> files = new List<SftpScheduler.BLL.Models.RemoteFileInfo>();
            for (int i = 0; i < count; i++)
            {
                string fileName = Path.GetRandomFileName();
                string fullName = $"{remotePath}{fileName}";
                var remoteFileInfo = new SftpScheduler.BLL.Models.RemoteFileInfo
                {
                    FullName = fullName,
                    IsDirectory = isDirectory,
                    LastWriteTime = DateTime.UtcNow,
                    Length = Faker.RandomNumber.Next(1, 1000),
                    Name = fileName
                };
                files.Add(remoteFileInfo);
            }
            return files;
        }


        private IFileTransferService CreateFileTransferService(ILogger<FileTransferService>? logger = null, ICreateJobFileLogCommand? createJobFileLogCommand = null, IDirectoryUtility? directoryWrap = null, IFileUtility? fileWrap = null)
        {
            return new FileTransferService(logger ?? Substitute.For<ILogger<FileTransferService>>()
                , createJobFileLogCommand ?? Substitute.For<ICreateJobFileLogCommand>()
				, directoryWrap ?? Substitute.For<IDirectoryUtility>()
                , fileWrap ?? Substitute.For<IFileUtility>()
                );
        }

        #endregion
    }

}
