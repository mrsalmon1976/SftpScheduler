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

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            fileTransferService.DownloadFiles(sessionWrapper, options);

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

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            fileTransferService.DownloadFiles(sessionWrapper, options);

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
            
            var remoteFiles = this.CreateRemoteFileList(1, options.RemotePath, false);
            sessionWrapper.ListDirectory(options.RemotePath).Returns(remoteFiles);
            string remoteFileName = remoteFiles[0].FullName;


            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            fileTransferService.DownloadFiles(sessionWrapper, options);

            // assert
            sessionWrapper.Received(1).GetFiles(remoteFileName, Arg.Any<string>(), deleteAfterDownload);
        }

        [Test]
        public void DownloadFiles_FileDownloadWithDelete_NoMoveOccurs()
        {
            // setup
            DownloadOptions options = CreateDownloadOptions(deleteAfterDownload: true);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();

            int fileCount = Faker.RandomNumber.Next(2, 7);
            var remoteFiles = this.CreateRemoteFileList(fileCount, options.RemotePath, false);
            sessionWrapper.ListDirectory(options.RemotePath).Returns(remoteFiles);

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            fileTransferService.DownloadFiles(sessionWrapper, options);

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

            int fileCount = Faker.RandomNumber.Next(2, 7);
            var remoteFiles = this.CreateRemoteFileList(fileCount, options.RemotePath, false);
            sessionWrapper.ListDirectory(options.RemotePath).Returns(remoteFiles);

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            fileTransferService.DownloadFiles(sessionWrapper, options);

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

            var remoteFiles = this.CreateRemoteFileList(1, options.RemotePath, false);
            sessionWrapper.ListDirectory(options.RemotePath).Returns(remoteFiles);

            // execute
            IFileUtility fileWrap = Substitute.For<IFileUtility>();
            IFileTransferService fileTransferService = CreateFileTransferService(fileWrap: fileWrap);
            fileTransferService.DownloadFiles(sessionWrapper, options);

            // assert

            foreach (string localCopyFolder in options.LocalCopyPaths)
            {
                string fileName = remoteFiles[0].Name;
                string sourcePath = Path.Combine(options.LocalPath, fileName);
                string targetPath = Path.Combine(localCopyFolder, fileName);
                fileWrap.Received(1).Copy(sourcePath, targetPath);
            }

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

            Assert.That(0, Is.EqualTo(availableFiles.Count()));
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

            Assert.That(fileCount, Is.EqualTo(availableFiles.Count()));
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

            IFileUtility fileWrap = Substitute.For<IFileUtility>();

            IFileTransferService fileTransferService = CreateFileTransferService(directoryWrap: directoryWrap, fileWrap: fileWrap);
            fileTransferService.UploadFiles(sessionWrapper, newFiles, remotePath);


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

            IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
            directoryWrap.EnumerateFiles(localPath).Returns(newFiles);

            IFileUtility fileWrap = Substitute.For<IFileUtility>();

            string expectedRename1 = localFileName.Replace(localFileExt, $".uploaded{localFileExt}");
            string expectedRename2 = localFileName.Replace(localFileExt, $".uploaded_1{localFileExt}");
            fileWrap.Exists(expectedRename1).Returns(true);

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(directoryWrap: directoryWrap, fileWrap: fileWrap);
            fileTransferService.UploadFiles(sessionWrapper, newFiles, remotePath);


            // assert
            fileWrap.Received(1).Exists(expectedRename1);
            fileWrap.Received(1).Move(localFileName, expectedRename2, false);
        }


        [Test]
        public void UploadFiles_FilesAvailableForUpload_FilesUploaded()
        {
            List<string> newFiles = CreateLocalFileList(Faker.RandomNumber.Next(6, 9), false);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            string localPath = AppDomain.CurrentDomain.BaseDirectory;
            string remotePath = $"/{Path.GetRandomFileName()}/";

            IDirectoryUtility directoryWrap = Substitute.For<IDirectoryUtility>();
            directoryWrap.EnumerateFiles(localPath).Returns(newFiles);

            IFileUtility fileWrap = Substitute.For<IFileUtility>();

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(directoryWrap: directoryWrap, fileWrap: fileWrap);
            fileTransferService.UploadFiles(sessionWrapper, newFiles, remotePath);

            // assert
            sessionWrapper.Received(newFiles.Count).PutFiles(Arg.Any<string>(), remotePath);
            foreach (string file in newFiles)
            {
                sessionWrapper.Received(1).PutFiles(file, remotePath);
            }
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


        private IFileTransferService CreateFileTransferService(ILogger<FileTransferService>? logger = null, IDirectoryUtility? directoryWrap = null, IFileUtility? fileWrap = null)
        {
            return new FileTransferService(logger ?? Substitute.For<ILogger<FileTransferService>>()
                , directoryWrap ?? Substitute.For<IDirectoryUtility>()
                , fileWrap ?? Substitute.For<IFileUtility>()
                );
        }

        #endregion
    }

}
