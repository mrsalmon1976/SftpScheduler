using NSubstitute;
using NUnit.Framework;
using SftpScheduler.Common.IO;
using SftpSchedulerService.AutoUpdater.Services;
using SftpScheduler.Common;
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
    public class UpdateFileServiceTest
    {

        #region Backup Tests

        [Test]
        public void Backup_OnExecute_DeletesAndCopiesRecursively()
        {
            // setup
            IDirectoryUtility dirUtility = Substitute.For<IDirectoryUtility>();
            UpdateLocationInfo updateLocationInfo = new UpdateLocationInfo();

            // execute
            IUpdateFileService updateFileService = CreateUpdateFileService(dirUtility: dirUtility);
            updateFileService.Backup(updateLocationInfo).Wait();

            // assert
            dirUtility.Received(1).Delete(updateLocationInfo.BackupFolder, UpdateFileService.MaxDeleteRetryCount);
            dirUtility.Received(1).CopyRecursive(updateLocationInfo.ApplicationFolder, updateLocationInfo.BackupFolder, Arg.Any<IEnumerable<string>>());
        }

        [Test]
        public void Backup_OnExecute_ExcludesBackupAndTempFolder()
        {
            // setup
            IDirectoryUtility dirUtility = Substitute.For<IDirectoryUtility>();
            UpdateLocationInfo updateLocationInfo = new UpdateLocationInfo();


            IEnumerable<string>? receivedExclusions = null;
            dirUtility.CopyRecursive(Arg.Any<string>(), Arg.Any<string>(), Arg.Do<IEnumerable<string>>(x => receivedExclusions = x));

            // execute
            IUpdateFileService updateFileService = CreateUpdateFileService(dirUtility: dirUtility);
            updateFileService.Backup(updateLocationInfo).Wait();

            // assert
            dirUtility.Received(1).CopyRecursive(updateLocationInfo.ApplicationFolder, updateLocationInfo.BackupFolder, Arg.Any<IEnumerable<string>>());
            Assert.That(receivedExclusions, Is.Not.Null);
            Assert.IsTrue(receivedExclusions!.Contains(updateLocationInfo.BackupFolder));
            Assert.IsTrue(receivedExclusions!.Contains(updateLocationInfo.UpdateTempFolder));

        }

        #endregion

        #region CopyNewVersionFiles

        [Test]
        public void CopyNewVersionFiles_OnExecute_CopiesFilesFromTempFolderToInstallationFolder()
        {
            // setup
            IDirectoryUtility dirUtility = Substitute.For<IDirectoryUtility>();
            UpdateLocationInfo updateLocationInfo = new UpdateLocationInfo();

            // execute
            IUpdateFileService updateFileService = CreateUpdateFileService(dirUtility: dirUtility);
            updateFileService.CopyNewVersionFiles(updateLocationInfo).Wait();

            // assert
            dirUtility.Received(1).CopyRecursive(updateLocationInfo.UpdateTempFolder, updateLocationInfo.ApplicationFolder, Arg.Any<string[]>());
        }

        #endregion 

        #region DeleteCurrentVersionFiles Tests

        [Test]
        public void DeleteCurrentVersionFiles_OnExecute_DeletesContentsOfApplicationFolder()
        {
            // setup
            IDirectoryUtility dirUtility = Substitute.For<IDirectoryUtility>();
            UpdateLocationInfo updateLocationInfo = new UpdateLocationInfo();

            // execute
            IUpdateFileService updateFileService = CreateUpdateFileService(dirUtility: dirUtility);
            updateFileService.DeleteCurrentVersionFiles(updateLocationInfo).Wait();

            // assert
            dirUtility.Received(1).DeleteContents(updateLocationInfo.ApplicationFolder, Arg.Any<IEnumerable<string>>(), UpdateFileService.MaxDeleteRetryCount);
        }

        [Test]
        public void DeleteCurrentVersionFiles_OnExecute_ExcludesTemporaryAndDataFolders()
        {
            // setup
            IDirectoryUtility dirUtility = Substitute.For<IDirectoryUtility>();
            UpdateLocationInfo updateLocationInfo = new UpdateLocationInfo();


            IEnumerable<string>? receivedExclusions = null;
            dirUtility.DeleteContents(Arg.Any<string>(), Arg.Do<IEnumerable<string>>(x => receivedExclusions = x));

            // execute
            IUpdateFileService updateFileService = CreateUpdateFileService(dirUtility: dirUtility);
            updateFileService.DeleteCurrentVersionFiles(updateLocationInfo).Wait();

            // assert
            dirUtility.Received(1).DeleteContents(updateLocationInfo.ApplicationFolder, Arg.Any<IEnumerable<string>>());
            Assert.That(receivedExclusions, Is.Not.Null);
            Assert.IsTrue(receivedExclusions!.Contains(updateLocationInfo.BackupFolder));
            Assert.IsTrue(receivedExclusions!.Contains(updateLocationInfo.UpdateTempFolder));
            Assert.IsTrue(receivedExclusions!.Contains(updateLocationInfo.DataFolder));
            Assert.IsTrue(receivedExclusions!.Contains(updateLocationInfo.LogFolder));
            Assert.IsTrue(receivedExclusions!.Contains(updateLocationInfo.AutoUpdaterFolder));
            Assert.AreEqual(5, receivedExclusions!.Count());

        }

        #endregion

        #region EnsureEmptyUpdateTempFolderExists Tests

        [Test]
        public void EnsureEmptyUpdateTempFolderExists_FolderDoesNotExist_CreatesFolder()
        {
            // setup
            string updateTempFolder = "C:\\Temp\\Test";
            IDirectoryUtility dirUtility = Substitute.For<IDirectoryUtility>();
            dirUtility.Exists(updateTempFolder).Returns(false); 

            // execute
            IUpdateFileService updateFileService = CreateUpdateFileService(dirUtility: dirUtility);
            updateFileService.EnsureEmptyUpdateTempFolderExists(updateTempFolder);

            // assert
            dirUtility.DidNotReceive().Delete(Arg.Any<string>(), Arg.Any<int>());
            dirUtility.Received(1).Create(updateTempFolder);
        }

        [Test]
        public void EnsureEmptyUpdateTempFolderExists_FolderAlreadyExists_DeletesOldAndCreatesEmptyFolder()
        {
            // setup
            string updateTempFolder = "C:\\Temp\\Test";
            IDirectoryUtility dirUtility = Substitute.For<IDirectoryUtility>();
            dirUtility.Exists(updateTempFolder).Returns(true);

            // execute
            IUpdateFileService updateFileService = CreateUpdateFileService(dirUtility: dirUtility);
            updateFileService.EnsureEmptyUpdateTempFolderExists(updateTempFolder);

            // assert
            dirUtility.Received(1).Delete(updateTempFolder, UpdateFileService.MaxDeleteRetryCount);
            dirUtility.Received(1).Create(updateTempFolder);

        }

        #endregion

        #region ExtractReleasePackage Tests

        [Test]
        public void ExtractReleasePackage_OnExecute_ExtractsZipFile()
        {
            // setup
            string zipFilePath = GetFakePath("Release.zip");
            string extractFolder = GetFakePath("ExtractFolder");
            IFileUtility fileUtility = Substitute.For<IFileUtility>();

            // execute
            IUpdateFileService updateFileService = CreateUpdateFileService(fileUtility: fileUtility);
            updateFileService.ExtractReleasePackage(zipFilePath, extractFolder).Wait();

            // assert
            fileUtility.Received(1).ExtractZipFile(zipFilePath, extractFolder);
        }

        [Test]
        public void ExtractReleasePackage_OnExecute_RenamesAutoUpdaterFilesWithTempExtension()
        {
            // setup
            string zipFilePath = GetFakePath("Release.zip");
            string extractFolder = GetFakePath("ExtractFolder");
            string autoUpdateFolder = Path.Combine(extractFolder, UpdateConstants.AutoUpdaterFolderName);
            IFileUtility fileUtility = Substitute.For<IFileUtility>();
            IDirectoryUtility dirUtility = Substitute.For<IDirectoryUtility>();

            string file1 = Path.Combine(autoUpdateFolder, "file1.txt");
            string file2 = Path.Combine(autoUpdateFolder, "file2.txt");
            string[] autoUpdaterFiles = { file1, file2 };
            dirUtility.Exists(autoUpdateFolder).Returns(true);
            dirUtility.GetFiles(autoUpdateFolder, SearchOption.AllDirectories).Returns(autoUpdaterFiles);

            // execute
            IUpdateFileService updateFileService = CreateUpdateFileService(fileUtility, dirUtility);
            updateFileService.ExtractReleasePackage(zipFilePath, extractFolder).Wait();

            // assert
            dirUtility.Received(1).Exists(autoUpdateFolder);
            dirUtility.Received(1).GetFiles(autoUpdateFolder, SearchOption.AllDirectories);
            fileUtility.Received(1).Move(file1, $"{file1}{UpdateConstants.AutoUpdaterNewFileExtension}", true);
            fileUtility.Received(1).Move(file2, $"{file2}{UpdateConstants.AutoUpdaterNewFileExtension}", true);
        }

        [Test]
        public void ExtractReleasePackage_OnExecute_DeletesZipFile()
        {
            // setup
            string zipFilePath = GetFakePath("Release.zip");
            string extractFolder = GetFakePath("ExtractFolder");
            IFileUtility fileUtility = Substitute.For<IFileUtility>();

            // execute
            IUpdateFileService updateFileService = CreateUpdateFileService(fileUtility);
            updateFileService.ExtractReleasePackage(zipFilePath, extractFolder).Wait();

            // assert
            fileUtility.Received(1).Delete(zipFilePath);
        }

        #endregion

        private string GetFakePath(string fileOrFolderName)
        {
            return Path.Combine(Assembly.GetExecutingAssembly().Location, fileOrFolderName);
        }

        private IUpdateFileService CreateUpdateFileService(IFileUtility? fileUtility = null, IDirectoryUtility? dirUtility = null)
        {
            return new UpdateFileService(
                fileUtility ?? Substitute.For<IFileUtility>()
                , dirUtility ?? Substitute.For<IDirectoryUtility>()
                );
        }

    }
}
