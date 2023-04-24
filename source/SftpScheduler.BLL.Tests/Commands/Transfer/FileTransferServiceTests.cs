using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Utility.IO;
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
        public void DownloadFiles_MultipleFiles_VerifyDownloads()
        {
            // setup
            int fileCount = Faker.RandomNumber.Next(2, 10);
            string localPath = "C:\\Temp";
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            List<string> filesToDownload = CreateRemoteFileList(fileCount, "/Test/", false).Select(x => x.FullName).ToList();

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            fileTransferService.DownloadFiles(sessionWrapper, filesToDownload, localPath, false);

            // assert
            sessionWrapper.Received(fileCount).GetFiles(Arg.Any<string>(), Arg.Any<string>(), false);
            foreach (string remotePath in filesToDownload)
            {
                string fileName = Path.GetFileName(remotePath);
                string localFilePath = Path.Combine(localPath, fileName);

                sessionWrapper.Received(1).GetFiles(remotePath, localFilePath, false);
            }

        }

        [TestCase(true)]
        [TestCase(false)]
        public void DownloadFiles_FileDownload_ObeysDelete(bool deleteAfterDownload)
        {
            // setup
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            List<string> filesToDownload = CreateRemoteFileList(1, "/Test/", false).Select(x => x.FullName).ToList();
            string remoteFileName = filesToDownload[0];
            
            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            fileTransferService.DownloadFiles(sessionWrapper, filesToDownload, "C:\\Temp", deleteAfterDownload);

            // assert
            sessionWrapper.Received(1).GetFiles(remoteFileName, Arg.Any<string>(), deleteAfterDownload);
        }

        #endregion

        #region DownloadFilesAvailable Tests

        [Test]
        public void DownloadFilesAvailable_NoFiles_ReturnsEmptyCollection()
        {
            // setup
            string remotePath = "/Test/";
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapper.ListDirectory(remotePath).Returns(Enumerable.Empty<RemoteFileInfo>());

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            IEnumerable<string> files = fileTransferService.DownloadFilesAvailable(sessionWrapper, remotePath);

            //assert
            sessionWrapper.Received(1).ListDirectory(remotePath);
            Assert.That(0, Is.EqualTo(files.Count()));
        }

        [Test]
        public void DownloadFilesAvailable_MultipleFiles_ReturnsFiles()
        {
            // setup
            string remotePath = "/Test/";
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            int fileCount = Faker.RandomNumber.Next(2, 10);
            var remoteFiles = this.CreateRemoteFileList(fileCount, remotePath, false);
            sessionWrapper.ListDirectory(remotePath).Returns(remoteFiles);

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            IEnumerable<string> files = fileTransferService.DownloadFilesAvailable(sessionWrapper, remotePath);

            //assert
            sessionWrapper.Received(1).ListDirectory(remotePath);
            Assert.That(fileCount, Is.EqualTo(files.Count()));

        }

        [Test]
        public void DownloadFilesAvailable_DirectoriesOnRemote_OnlyFilesReturned()
        {
            // setup
            string remotePath = "/Test/";
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            int fileCount = Faker.RandomNumber.Next(2, 10);
            var remoteFiles = this.CreateRemoteFileList(fileCount, remotePath, false);
            remoteFiles.AddRange(this.CreateRemoteFileList(2, remotePath, true));
            sessionWrapper.ListDirectory(remotePath).Returns(remoteFiles);

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService();
            IEnumerable<string> files = fileTransferService.DownloadFilesAvailable(sessionWrapper, remotePath);

            //assert
            sessionWrapper.Received(1).ListDirectory(remotePath);
            Assert.That(fileCount, Is.EqualTo(files.Count()));

        }

        #endregion

        #region UploadFilesAvailable Tests

        [Test]
        public void UploadFilesAvailable_NoNewFiles_NoFilesReturned()
        {
            int fileCount = Faker.RandomNumber.Next(2, 9);
            IEnumerable<string> files = CreateLocalFileList(fileCount, true);

            string localPath = AppDomain.CurrentDomain.BaseDirectory;

            IDirectoryWrap directoryWrap = Substitute.For<IDirectoryWrap>();
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

            IDirectoryWrap directoryWrap = Substitute.For<IDirectoryWrap>();
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

            IDirectoryWrap directoryWrap = Substitute.For<IDirectoryWrap>();
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

            IDirectoryWrap directoryWrap = Substitute.For<IDirectoryWrap>();
            directoryWrap.EnumerateFiles(localPath).Returns(newFiles);

            IFileWrap fileWrap = Substitute.For<IFileWrap>();

            IFileTransferService fileTransferService = CreateFileTransferService(directoryWrap: directoryWrap, fileWrap: fileWrap);
            fileTransferService.UploadFiles(sessionWrapper, newFiles, remotePath);


            fileWrap.Received(newFiles.Count).Move(Arg.Any<string>(), Arg.Any<string>());
            foreach (string file in newFiles)
            {
                fileWrap.Received(1).Move(file, Arg.Any<string>());
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

            IDirectoryWrap directoryWrap = Substitute.For<IDirectoryWrap>();
            directoryWrap.EnumerateFiles(localPath).Returns(newFiles);

            IFileWrap fileWrap = Substitute.For<IFileWrap>();

            string expectedRename1 = localFileName.Replace(localFileExt, $".uploaded{localFileExt}");
            string expectedRename2 = localFileName.Replace(localFileExt, $".uploaded_1{localFileExt}");
            fileWrap.Exists(expectedRename1).Returns(true);

            // execute
            IFileTransferService fileTransferService = CreateFileTransferService(directoryWrap: directoryWrap, fileWrap: fileWrap);
            fileTransferService.UploadFiles(sessionWrapper, newFiles, remotePath);


            // assert
            fileWrap.Received(1).Exists(expectedRename1);
            fileWrap.Received(1).Move(localFileName, expectedRename2);
        }


        [Test]
        public void UploadFiles_FilesAvailableForUpload_FilesUploaded()
        {
            List<string> newFiles = CreateLocalFileList(Faker.RandomNumber.Next(6, 9), false);

            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            string localPath = AppDomain.CurrentDomain.BaseDirectory;
            string remotePath = $"/{Path.GetRandomFileName()}/";

            IDirectoryWrap directoryWrap = Substitute.For<IDirectoryWrap>();
            directoryWrap.EnumerateFiles(localPath).Returns(newFiles);

            IFileWrap fileWrap = Substitute.For<IFileWrap>();

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


        private IFileTransferService CreateFileTransferService(ILogger<FileTransferService>? logger = null, IDirectoryWrap? directoryWrap = null, IFileWrap? fileWrap = null)
        {
            return new FileTransferService(logger ?? Substitute.For<ILogger<FileTransferService>>()
                , directoryWrap ?? Substitute.For<IDirectoryWrap>()
                , fileWrap ?? Substitute.For<IFileWrap>()
                );
        }

        #endregion
    }

}
