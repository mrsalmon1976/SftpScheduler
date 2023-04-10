using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Utility.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace SftpScheduler.BLL.Tests.Commands.Transfer
{
    [TestFixture]
    public class FileTransferServiceTests
    {
        #region UploadFilesAvailable Tests

        [Test]
        public void UploadFilesAvailable_NoNewFiles_NoFilesReturned()
        {
            int fileCount = Faker.RandomNumber.Next(2, 9);
            IEnumerable<string> files = CreateFileList(fileCount, true);

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
            IEnumerable<string> files = CreateFileList(fileCount, false);

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
            IEnumerable<string> uploadedFiles = CreateFileList(Faker.RandomNumber.Next(2, 5), true);
            List<string> newFiles = CreateFileList(Faker.RandomNumber.Next(6, 9), false);
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
            List<string> newFiles = CreateFileList(Faker.RandomNumber.Next(6, 9), false);

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
        public void UploadFiles_FilesAvailableForUpload_FilesUploaded()
        {
            List<string> newFiles = CreateFileList(Faker.RandomNumber.Next(6, 9), false);

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
            sessionWrapper.Received(newFiles.Count).PutFiles(Arg.Any<string>(), remotePath, Arg.Any<TransferOptions>());
            foreach (string file in newFiles)
            {
                sessionWrapper.Received(1).PutFiles(file, remotePath, Arg.Any<TransferOptions>());
            }
        }

        #endregion

        #region Private Methods

        private List<string> CreateFileList(int count, bool isUploaded)
        {
            List<string> files = new List<string>();
            string uploadedToken = (isUploaded ? ".uploaded" : "");
            for (int i=0; i<count; i++)
            {
                string fileName = $"C:\\Temp\\file_{i}{uploadedToken}.zip";
                files.Add(fileName);
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
