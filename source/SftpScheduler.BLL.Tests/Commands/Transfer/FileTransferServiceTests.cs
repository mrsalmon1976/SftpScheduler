using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.Common.IO;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.Test.Common;
using SftpScheduler.Common;

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
            sessionWrapper.DidNotReceive().GetFiles(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DownloadOptions>());
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
            sessionWrapper.Received(fileCount).GetFiles(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DownloadOptions>());

        }


        [Test]
        public void DownloadFiles_FileDownload_PassesOptions()
        {
            // setup
            DownloadOptions options = new SubstituteBuilder<DownloadOptions>().WithRandomProperties().Build();

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            IDbContext dbContext = Substitute.For<IDbContext>();

            var remoteFiles = this.CreateRemoteFileList(1, options.RemotePath, false);
            sessionWrapper.ListDirectory(options.RemotePath).Returns(remoteFiles);
            string remoteFileName = remoteFiles[0].FullName;


            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            fileTransferService.DownloadFiles(sessionWrapper, dbContext, options);

            // assert
            sessionWrapper.Received(1).GetFiles(remoteFileName, Arg.Any<string>(), options);
        }

        [Test]
        public void DownloadFiles_FilesFoundButNotDownloaded_LoggingDoesNotOccur()
        {
            // setup
            DownloadOptions options = new SubstituteBuilder<DownloadOptions>().WithRandomProperties().Build();

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            IDbContext dbContext = Substitute.For<IDbContext>();

            ICreateJobFileLogCommand createJobFileLogCommand = new SubstituteBuilder<ICreateJobFileLogCommand>().Build();

            var remoteFiles = this.CreateRemoteFileList(1, options.RemotePath, false);
            sessionWrapper.ListDirectory(options.RemotePath).Returns(remoteFiles);

            sessionWrapper.GetFiles(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DownloadOptions>()).Returns(0);

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(createJobFileLogCommand: createJobFileLogCommand);
            fileTransferService.DownloadFiles(sessionWrapper, dbContext, options);

            // assert
            sessionWrapper.Received(1).GetFiles(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DownloadOptions>());
            createJobFileLogCommand.DidNotReceive().ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobFileLogEntity>());
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
            sessionWrapper.GetFiles(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DownloadOptions>()).Returns(1);

            ICreateJobFileLogCommand createJobFileLogCommand = new SubstituteBuilder<ICreateJobFileLogCommand>().Build();

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(createJobFileLogCommand: createJobFileLogCommand);
            fileTransferService.DownloadFiles(sessionWrapper, dbContext, options);

            // assert
            sessionWrapper.Received(fileCount).GetFiles(Arg.Any<string>(), Arg.Any<string>(), options);
            sessionWrapper.DidNotReceive().MoveFile(Arg.Any<string>(), Arg.Any<string>());
            createJobFileLogCommand.Received(fileCount).ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobFileLogEntity>());
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
            sessionWrapper.GetFiles(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DownloadOptions>()).Returns(1);

            foreach (RemoteFileInfo remoteFileInfo in remoteFiles) 
            {
                string result = $"{options.RemoteArchivePath}{remoteFileInfo.Name}";
                sessionWrapper.GetUniquePath(options.RemoteArchivePath!, remoteFileInfo.Name).Returns(result);
            }

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            fileTransferService.DownloadFiles(sessionWrapper, dbContext, options);

            // assert
            sessionWrapper.Received(fileCount).GetFiles(Arg.Any<string>(), Arg.Any<string>(), options);
            sessionWrapper.Received(fileCount).GetUniquePath(Arg.Any<string>(), Arg.Any<string>());
            sessionWrapper.Received(fileCount).MoveFile(Arg.Any<string>(), Arg.Any<string>());

            foreach (RemoteFileInfo downloadFile in remoteFiles)
            {
                string targetPath = $"{options.RemoteArchivePath}{downloadFile.Name}";
                sessionWrapper.Received(1).GetUniquePath(options.RemoteArchivePath!, downloadFile.Name);
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
            sessionWrapper.GetFiles(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DownloadOptions>()).Returns(1);

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
            sessionWrapper.GetFiles(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DownloadOptions>()).Returns(1);

            int fileLength = Faker.RandomNumber.Next(1, 1000);
            IFileUtility fileWrap = Substitute.For<IFileUtility>();
            fileWrap.GetFileSize(Arg.Any<string>()).Returns(fileLength);

            IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
            DateTime startDate = DateTime.Now;
            dateTimeProvider.Now.Returns(startDate);


            // execute
            ICreateJobFileLogCommand createJobFileLogCmd = Substitute.For<ICreateJobFileLogCommand>();
            JobFileLogEntity? logEntity = null;
            createJobFileLogCmd.When(x => x.ExecuteAsync(dbContext, Arg.Any<JobFileLogEntity>())).Do(ci =>
            {
                logEntity = ci.ArgAt<JobFileLogEntity>(1);
            });

            IFileTransferService fileTransferService = CreateFileTransferService(createJobFileLogCommand: createJobFileLogCmd, fileWrap: fileWrap, dateTimeProvider: dateTimeProvider);
            fileTransferService.DownloadFiles(sessionWrapper, dbContext, options);

            // assert
            createJobFileLogCmd.Received(1).ExecuteAsync(dbContext, Arg.Any<JobFileLogEntity>());
            Assert.That(logEntity, Is.Not.Null);
            Assert.That(logEntity.JobId, Is.EqualTo(jobId));
            Assert.That(logEntity.FileName, Is.EqualTo(remoteFiles[0].Name));
            Assert.That(logEntity.FileLength, Is.EqualTo(fileLength));
            Assert.That(logEntity.StartDate, Is.EqualTo(startDate));
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
            // setup
            List<string> newFiles = CreateLocalFileList(Faker.RandomNumber.Next(6, 9), false);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapper.PutFiles(Arg.Any<string>(), Arg.Any<UploadOptions>()).Returns(1);
            string localPath = AppDomain.CurrentDomain.BaseDirectory;
            string remotePath = $"/{Path.GetRandomFileName()}/";

            IDbContext dbContext = Substitute.For<IDbContext>();
            IFileUtility fileWrap = Substitute.For<IFileUtility>();

            UploadOptions options = new SubstituteBuilder<UploadOptions>()
                .WithRandomProperties()
                .WithProperty(x => x.LocalFilePaths, newFiles)
                .Build();

            IUploadCompressionService uploadCompressionService = CreatePreparedUploadCompressionService();

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(uploadCompressionService: uploadCompressionService, fileWrap: fileWrap);
            fileTransferService.UploadFiles(sessionWrapper, dbContext, options);


            fileWrap.Received(newFiles.Count).Move(Arg.Any<string>(), Arg.Any<string>(), false);
            foreach (string file in newFiles)
            {
                fileWrap.Received(1).Move(file, Arg.Any<string>(), false);
            }
        }

        [Test]
        public void UploadFiles_CompressionCalled()
        {
            // setup
            List<string> newFiles = CreateLocalFileList(1, false);
            string localPath = newFiles[0];

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapper.PutFiles(Arg.Any<string>(), Arg.Any<UploadOptions>()).Returns(1);

            IDbContext dbContext = Substitute.For<IDbContext>();

            UploadOptions options = new SubstituteBuilder<UploadOptions>()
                .WithRandomProperties()
                .WithProperty(x => x.LocalFilePaths, newFiles)
                .Build();

            CompressedFileInfo compressedFileInfo = new SubstituteBuilder<CompressedFileInfo>().WithRandomProperties().Build();
            IUploadCompressionService uploadCompressionService = new SubstituteBuilder<IUploadCompressionService>().Build();
            uploadCompressionService.PrepareUploadFile(localPath, options.CompressionMode).Returns(compressedFileInfo);

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(uploadCompressionService: uploadCompressionService);
            fileTransferService.UploadFiles(sessionWrapper, dbContext, options);

            // assert
            uploadCompressionService.Received(1).PrepareUploadFile(localPath, options.CompressionMode);
            uploadCompressionService.Received(1).RemoveCompressedFile(compressedFileInfo);
        }

        [Test]
        public void UploadFiles_FileCompressed_UploadsCompressedFile()
        {
            // setup
            List<string> newFiles = CreateLocalFileList(1, false);
            string localPath = newFiles[0];

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapper.PutFiles(Arg.Any<string>(), Arg.Any<UploadOptions>()).Returns(1);

            IDbContext dbContext = Substitute.For<IDbContext>();

            UploadOptions options = new SubstituteBuilder<UploadOptions>()
                .WithRandomProperties()
                .WithProperty(x => x.LocalFilePaths, newFiles)
                .Build();

            CompressedFileInfo compressedFileInfo = new SubstituteBuilder<CompressedFileInfo>().WithRandomProperties().Build();
            IUploadCompressionService uploadCompressionService = new SubstituteBuilder<IUploadCompressionService>().Build();
            uploadCompressionService.PrepareUploadFile(localPath, options.CompressionMode).Returns(compressedFileInfo);

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(uploadCompressionService: uploadCompressionService);
            fileTransferService.UploadFiles(sessionWrapper, dbContext, options);

            // assert
            sessionWrapper.Received(1).PutFiles(compressedFileInfo.CompressedFilePath!, options);
        }

        [Test]
        public void UploadFiles_FileNotCompressed_UploadsOriginalFile()
        {
            // setup
            List<string> newFiles = CreateLocalFileList(1, false);
            string localPath = newFiles[0];

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapper.PutFiles(Arg.Any<string>(), Arg.Any<UploadOptions>()).Returns(1);

            IDbContext dbContext = Substitute.For<IDbContext>();

            UploadOptions options = new SubstituteBuilder<UploadOptions>()
                .WithRandomProperties()
                .WithProperty(x => x.LocalFilePaths, newFiles)
                .Build();

            CompressedFileInfo compressedFileInfo = new SubstituteBuilder<CompressedFileInfo>()
                .WithRandomProperties()
                .WithProperty(x => x.CompressedFilePath, null)
                .Build();
            IUploadCompressionService uploadCompressionService = new SubstituteBuilder<IUploadCompressionService>().Build();
            uploadCompressionService.PrepareUploadFile(localPath, options.CompressionMode).Returns(compressedFileInfo);

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(uploadCompressionService: uploadCompressionService);
            fileTransferService.UploadFiles(sessionWrapper, dbContext, options);

            // assert
            sessionWrapper.Received(1).PutFiles(localPath, options);
        }


        [Test]
        public void UploadFiles_FilesAvailableForUploadExists_MarkedAsUploadedWithSuffix()
        {
            // setup
            List<string> newFiles = CreateLocalFileList(1, false);
            string localFileName = newFiles[0];
            string localFileExt = Path.GetExtension(localFileName);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapper.PutFiles(Arg.Any<string>(), Arg.Any<UploadOptions>()).Returns(1);
            string localPath = AppDomain.CurrentDomain.BaseDirectory;
            string remotePath = $"/{Path.GetRandomFileName()}/";

            IDbContext dbContext = Substitute.For<IDbContext>();

            IFileUtility fileWrap = Substitute.For<IFileUtility>();

            string expectedRename1 = localFileName.Replace(localFileExt, $".uploaded{localFileExt}");
            string expectedRename2 = localFileName.Replace(localFileExt, $".uploaded_1{localFileExt}");
            fileWrap.Exists(expectedRename1).Returns(true);

            UploadOptions options = new SubstituteBuilder<UploadOptions>()
                .WithRandomProperties()
                .WithProperty(x => x.LocalFilePaths, newFiles)
                .WithProperty(x => x.LocalArchivePath, String.Empty)
                .WithProperty(x => x.LocalPrefix, String.Empty)
                .Build();

            IUploadCompressionService uploadCompressionService = CreatePreparedUploadCompressionService();

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(uploadCompressionService: uploadCompressionService, fileWrap: fileWrap);
            fileTransferService.UploadFiles(sessionWrapper, dbContext, options);


            // assert
            fileWrap.Received(1).Exists(expectedRename1);
            fileWrap.Received(1).Move(localFileName, expectedRename2, false);
        }

        [Test]
        public void UploadFiles_FilesAvailableForUpload_NoLocalArchiveAndNoLocalPrefix_MarkedAsUploadedWithSuffix()
        {
            // setup
            List<string> newFiles = CreateLocalFileList(1, false);
            string localFileName = newFiles[0];
            string localFileExt = Path.GetExtension(localFileName);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapper.PutFiles(Arg.Any<string>(), Arg.Any<UploadOptions>()).Returns(1);
            string localPath = AppDomain.CurrentDomain.BaseDirectory;
            string remotePath = $"/{Path.GetRandomFileName()}/";

            IDbContext dbContext = Substitute.For<IDbContext>();

            IFileUtility fileWrap = Substitute.For<IFileUtility>();

            string expectedRename = localFileName.Replace(localFileExt, $".uploaded{localFileExt}");
            fileWrap.Exists(expectedRename).Returns(false);

            UploadOptions options = new SubstituteBuilder<UploadOptions>()
                .WithRandomProperties()
                .WithProperty(x => x.LocalFilePaths, newFiles)
                .WithProperty(x => x.LocalArchivePath, String.Empty)
                .WithProperty(x => x.LocalPrefix, String.Empty)
                .Build();

            IUploadCompressionService uploadCompressionService = CreatePreparedUploadCompressionService();

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(uploadCompressionService: uploadCompressionService, fileWrap: fileWrap);
            fileTransferService.UploadFiles(sessionWrapper, dbContext, options);


            // assert
            fileWrap.Received(1).Exists(expectedRename);
            fileWrap.Received(1).Move(localFileName, expectedRename, false);
        }

        [TestCase("aaa_")]
        [TestCase("{YEAR}_")]
        [TestCase("{YEAR}{MONTH}_")]
        [TestCase("{DAY}_")]
        [TestCase("{HOUR}:{minute}:{SECOND}")]
        public void UploadFiles_FilesAvailableForUpload_NoLocalArchiveAndLocalPrefix_MarkedAsUploadedWithSuffix(string localPrefix)
        {
            // setup
            List<string> newFiles = CreateLocalFileList(1, false);
            string localFileName = newFiles[0];
            string localFileExt = Path.GetExtension(localFileName);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapper.PutFiles(Arg.Any<string>(), Arg.Any<UploadOptions>()).Returns(1);
            string localPath = AppDomain.CurrentDomain.BaseDirectory;
            string remotePath = $"/{Path.GetRandomFileName()}/";

            IDbContext dbContext = Substitute.For<IDbContext>();

            IFileUtility fileWrap = Substitute.For<IFileUtility>();

            IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
            DateTime now = DateTime.Now;
            dateTimeProvider.Now.Returns(now);


            string expectedFileName = $"{Path.GetFileNameWithoutExtension(localFileName)}.uploaded{localFileExt}";
            string expectedRename = ReplacePrefixDateVariables(now, $"{Path.GetDirectoryName(localFileName)}\\{localPrefix}{expectedFileName}");

            fileWrap.Exists(expectedRename).Returns(false);

            UploadOptions options = new SubstituteBuilder<UploadOptions>()
                .WithRandomProperties()
                .WithProperty(x => x.LocalFilePaths, newFiles)
                .WithProperty(x => x.LocalArchivePath, String.Empty)
                .WithProperty(x => x.LocalPrefix, localPrefix)
                .Build();

            IUploadCompressionService uploadCompressionService = CreatePreparedUploadCompressionService();

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(uploadCompressionService: uploadCompressionService, fileWrap: fileWrap, dateTimeProvider: dateTimeProvider);
            fileTransferService.UploadFiles(sessionWrapper, dbContext, options);


            // assert
            fileWrap.Received(1).Exists(expectedRename);
            fileWrap.Received(1).Move(localFileName, expectedRename, false);
        }

        [Test]
        public void UploadFiles_FilesAvailableForUpload_WithLocalArchiveAndNoLocalPrefix_MarkedAsUploadedWithSuffix()
        {
            // setup
            List<string> newFiles = CreateLocalFileList(1, false);
            string localFileName = newFiles[0];
            string localFileExt = Path.GetExtension(localFileName);
            string localArchivePath = "C:\\Temp";

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapper.PutFiles(Arg.Any<string>(), Arg.Any<UploadOptions>()).Returns(1);
            string localPath = AppDomain.CurrentDomain.BaseDirectory;
            string remotePath = $"/{Path.GetRandomFileName()}/";

            IDbContext dbContext = Substitute.For<IDbContext>();

            IFileUtility fileWrap = Substitute.For<IFileUtility>();

            DateTime now = DateTime.Now;

            string expectedRename = $"{localArchivePath}\\{Path.GetFileName(localFileName)}"; 

            fileWrap.Exists(expectedRename).Returns(false);

            UploadOptions options = new SubstituteBuilder<UploadOptions>()
                .WithRandomProperties()
                .WithProperty(x => x.LocalFilePaths, newFiles)
                .WithProperty(x => x.LocalArchivePath, localArchivePath)
                .WithProperty(x => x.LocalPrefix, String.Empty)
                .Build();

            IUploadCompressionService uploadCompressionService = CreatePreparedUploadCompressionService();

            IDirectoryUtility dirUtility = Substitute.For<IDirectoryUtility>();
            dirUtility.Exists(localArchivePath).Returns(true);

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(uploadCompressionService: uploadCompressionService, fileWrap: fileWrap, directoryWrap: dirUtility);
            fileTransferService.UploadFiles(sessionWrapper, dbContext, options);

            // assert
            fileWrap.Received(1).Exists(expectedRename);
            fileWrap.Received(1).Move(localFileName, expectedRename, false);
        }

        [Test]
        public void UploadFiles_FileAvailableButNotUploaded_DoesNotLogResult()
        {
            // setup
            List<string> newFiles = CreateLocalFileList(1, false);

            IDbContext dbContext = new SubstituteBuilder<IDbContext>().Build();
            ISessionWrapper sessionWrapper = new SubstituteBuilder<ISessionWrapper>().Build();
            sessionWrapper.PutFiles(Arg.Any<string>(), Arg.Any<UploadOptions>()).Returns(0);
            ICreateJobFileLogCommand createJobFileLogCommand = Substitute.For<ICreateJobFileLogCommand>();

            UploadOptions options = new SubstituteBuilder<UploadOptions>()
                .WithRandomProperties()
                .WithProperty(x => x.LocalFilePaths, newFiles)
                .Build();

            IUploadCompressionService uploadCompressionService = CreatePreparedUploadCompressionService();

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(createJobFileLogCommand: createJobFileLogCommand, uploadCompressionService: uploadCompressionService);
            fileTransferService.UploadFiles(sessionWrapper, dbContext, options);

            // assert
            sessionWrapper.Received(1).PutFiles(Arg.Any<string>(), Arg.Any<UploadOptions>());
            createJobFileLogCommand.DidNotReceive().ExecuteAsync(Arg.Any<IDbContext>(), Arg.Any<JobFileLogEntity>());

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

            ISessionWrapper sessionWrapper = new SubstituteBuilder<ISessionWrapper>().Build();
            sessionWrapper.PutFiles(Arg.Any<string>(), Arg.Any<UploadOptions>()).Returns(1);

            IUploadCompressionService uploadCompressionService = CreatePreparedUploadCompressionService();


            int fileLength = Faker.RandomNumber.Next(1, 1000);
			IFileUtility fileWrap = Substitute.For<IFileUtility>();
            fileWrap.GetFileSize(Arg.Any<string>()).Returns(fileLength);

            IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
            dateTimeProvider.Now.Returns(testStartDate);

            ICreateJobFileLogCommand createJobFileLogCommand = Substitute.For<ICreateJobFileLogCommand>();
            JobFileLogEntity? logEntity = null;
			createJobFileLogCommand.When(x => x.ExecuteAsync(dbContext, Arg.Any<JobFileLogEntity>())).Do(ci =>
            {
                logEntity = ci.ArgAt<JobFileLogEntity>(1);
			});

            UploadOptions options = new SubstituteBuilder<UploadOptions>()
                .WithRandomProperties()
                .WithProperty(x => x.JobId, jobId)
                .WithProperty(x => x.LocalFilePaths, newFiles)
                .Build();

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(fileWrap: fileWrap, createJobFileLogCommand: createJobFileLogCommand, uploadCompressionService: uploadCompressionService, dateTimeProvider: dateTimeProvider);
			fileTransferService.UploadFiles(sessionWrapper, dbContext, options);

            // assert
            Assert.That(logEntity, Is.Not.Null);
			Assert.That(logEntity.JobId, Is.EqualTo(jobId));
			Assert.That(logEntity.FileName, Is.EqualTo(Path.GetFileName(newFiles[0])));
			Assert.That(logEntity.FileLength, Is.EqualTo(fileLength));
			Assert.That(logEntity.StartDate, Is.EqualTo(testStartDate));
			Assert.That(logEntity.EndDate, Is.GreaterThanOrEqualTo(logEntity.StartDate));

            createJobFileLogCommand.Received(1).ExecuteAsync(dbContext, logEntity);

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
            IUploadCompressionService uploadCompressionService = CreatePreparedUploadCompressionService();

            UploadOptions options = new SubstituteBuilder<UploadOptions>()
                .WithRandomProperties()
                .WithProperty(x => x.RemotePath, remotePath)
                .WithProperty(x => x.LocalFilePaths, newFiles)
                .Build();

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(uploadCompressionService: uploadCompressionService, directoryWrap: directoryWrap, fileWrap: fileWrap);
			fileTransferService.UploadFiles(sessionWrapper, dbContext, options);

            // assert
            sessionWrapper.Received(newFiles.Count).PutFiles(Arg.Any<string>(), options);
            foreach (string file in newFiles)
            {
                sessionWrapper.Received(1).PutFiles(file, options);
            }
        }

        [Test]
        public void UploadFiles_WhenCallingPutFiles_SetsOptionsCorrectly()
        {
            List<string> newFiles = CreateLocalFileList(1, false);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            IDbContext dbContext = Substitute.For<IDbContext>();

            IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
            directoryWrap.EnumerateFiles(Arg.Any<string>()).Returns(newFiles);

            UploadOptions options = new SubstituteBuilder<UploadOptions>().WithRandomProperties().WithProperty(x => x.LocalFilePaths, newFiles).Build();

            IUploadCompressionService uploadCompressionService = CreatePreparedUploadCompressionService();

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(uploadCompressionService: uploadCompressionService, directoryWrap: directoryWrap);
            fileTransferService.UploadFiles(sessionWrapper, dbContext, options);

            // assert
            sessionWrapper.Received(newFiles.Count).PutFiles(Arg.Any<string>(), options);
        }


        [Test]
		public void UploadFiles_FilesAvailableForUpload_JobFileLogCommandExecuted()
		{
			List<string> newFiles = CreateLocalFileList(Faker.RandomNumber.Next(6, 9), false);

			ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapper.PutFiles(Arg.Any<string>(), Arg.Any<UploadOptions>()).Returns(1);

            IDbContext dbContext = Substitute.For<IDbContext>();    
			string localPath = AppDomain.CurrentDomain.BaseDirectory;
			string remotePath = $"/{Path.GetRandomFileName()}/";

			IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
			directoryWrap.EnumerateFiles(localPath).Returns(newFiles);

			ICreateJobFileLogCommand createJobFileCmd = Substitute.For<ICreateJobFileLogCommand>();
            IUploadCompressionService uploadCompressionService = CreatePreparedUploadCompressionService();

            UploadOptions options = new SubstituteBuilder<UploadOptions>()
                .WithRandomProperties()
                .WithProperty(x => x.LocalFilePaths, newFiles)
                .Build();

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(createJobFileLogCommand: createJobFileCmd, uploadCompressionService: uploadCompressionService, directoryWrap: directoryWrap);
			fileTransferService.UploadFiles(sessionWrapper, dbContext, options);

			// assert
			createJobFileCmd.Received(newFiles.Count).ExecuteAsync(dbContext, Arg.Any<JobFileLogEntity>());
		}
		#endregion

		#region Private Methods

		private DownloadOptions CreateDownloadOptions(int jobId = 0, string remotePath = "/Test/", string localPath = "C:\\Temp", bool deleteAfterDownload = false, string? remoteArchivePath = null, string? fileMask = null)
        {
            return new DownloadOptions()
            {
                JobId = jobId,
                RemotePath = remotePath,
                LocalPath = localPath,
                DeleteAfterDownload = deleteAfterDownload,
                RemoteArchivePath = remoteArchivePath,
                FileMask = fileMask ?? String.Empty
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


        private IFileTransferService CreateFileTransferService(ILogger<FileTransferService>? logger = null
            , ICreateJobFileLogCommand? createJobFileLogCommand = null
            , IUploadCompressionService? uploadCompressionService = null
            , IDirectoryUtility? directoryWrap = null
            , IFileUtility? fileWrap = null
            , IDateTimeProvider? dateTimeProvider = null)
        {
            return new FileTransferService(logger ?? Substitute.For<ILogger<FileTransferService>>()
                , createJobFileLogCommand ?? Substitute.For<ICreateJobFileLogCommand>()
                , uploadCompressionService ?? Substitute.For<IUploadCompressionService>()
                , directoryWrap ?? Substitute.For<IDirectoryUtility>()
                , fileWrap ?? Substitute.For<IFileUtility>()
                , dateTimeProvider ?? Substitute.For<IDateTimeProvider>()
                );
        }

        private IUploadCompressionService CreatePreparedUploadCompressionService()
        {
            CompressedFileInfo compressedFileInfo = new SubstituteBuilder<CompressedFileInfo>().Build();
            IUploadCompressionService uploadCompressionService = new SubstituteBuilder<IUploadCompressionService>().Build();
            uploadCompressionService.PrepareUploadFile(Arg.Any<string>(), Arg.Any<CompressionMode>()).Returns(compressedFileInfo);
            return uploadCompressionService;
        }

        private static string ReplacePrefixDateVariables(DateTime dt, string prefix)
        {
            const string format = "0#";
            string result = prefix;
            result = result.Replace("{YEAR}", dt.Year.ToString(), StringComparison.OrdinalIgnoreCase);
            result = result.Replace("{MONTH}", dt.Month.ToString(format), StringComparison.OrdinalIgnoreCase);
            result = result.Replace("{DAY}", dt.Day.ToString(format), StringComparison.OrdinalIgnoreCase);
            result = result.Replace("{HOUR}", dt.Hour.ToString(format), StringComparison.OrdinalIgnoreCase);
            result = result.Replace("{MINUTE}", dt.Minute.ToString(format), StringComparison.OrdinalIgnoreCase);
            result = result.Replace("{SECOND}", dt.Second.ToString(format), StringComparison.OrdinalIgnoreCase);
            return result;
        }

        #endregion
    }

}
