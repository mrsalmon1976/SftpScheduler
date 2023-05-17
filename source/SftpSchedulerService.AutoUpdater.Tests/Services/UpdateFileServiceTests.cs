using NSubstitute;
using NUnit.Framework;
using SftpScheduler.BLL.IO;
using SftpSchedulerService.AutoUpdater.Services;
using SftpSchedulerService.Common;
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

        //[Test]
        //public void CopyNewVersionFiles_OnExecute_CopiesFilesFromTempFolderToInstallationFolder()
        //{
        //    string newVersionFileName = Path.GetRandomFileName() + ".zip";
        //    string applicationFolder = GetFakePath("AppFolder");
        //    string updateTempFolder = GetFakePath("UpdateTemp");
        //    _updateLocationService.ApplicationFolder.Returns(applicationFolder);
        //    _updateLocationService.UpdateTempFolder.Returns(updateTempFolder);

        //    // execute
        //    _updateFileService.CopyNewVersionFiles(newVersionFileName).Wait();

        //    // assert
        //    _fileUtility.Received(1).CopyRecursive(updateTempFolder, applicationFolder, Arg.Any<string[]>());
        //}

        //[Test]
        //public void DeleteCurrentVersionFiles_OnExecute_DeletesContentsOfApplicationFolder()
        //{
        //    // setup
        //    string applicationFolder = GetFakePath("AppFolder");
        //    _updateLocationService.ApplicationFolder.Returns(applicationFolder);

        //    // execute
        //    _updateFileService.DeleteCurrentVersionFiles().Wait();

        //    // assert
        //    _fileUtility.Received(1).DeleteContents(applicationFolder, Arg.Any<IEnumerable<string>>());
        //}

        //[Test]
        //public void DeleteCurrentVersionFiles_OnExecute_ExcludesTemporaryAndDataFolders()
        //{
        //    // setup
        //    string backupFolder = GetFakePath("Backup");
        //    string applicationFolder = GetFakePath("AppFolder");
        //    string updateTempFolder = GetFakePath("UpdateTemp");
        //    string dataFolder = GetFakePath("DataVault");
        //    string updateEventLogFilePath = GetFakePath("Update.log");
        //    string autoUpdaterFolder = GetFakePath("AutoUpdater");
        //    _updateLocationService.BackupFolder.Returns(backupFolder);
        //    _updateLocationService.ApplicationFolder.Returns(applicationFolder);
        //    _updateLocationService.UpdateTempFolder.Returns(updateTempFolder);
        //    _updateLocationService.DataFolder.Returns(dataFolder);
        //    _updateLocationService.UpdateEventLogFilePath.Returns(updateEventLogFilePath);
        //    _updateLocationService.AutoUpdaterFolder.Returns(autoUpdaterFolder);


        //    IEnumerable<string> receivedExclusions = null;
        //    _fileUtility.DeleteContents(Arg.Any<string>(), Arg.Do<IEnumerable<string>>(x => receivedExclusions = x));

        //    // execute
        //    _updateFileService.DeleteCurrentVersionFiles().Wait();

        //    // assert
        //    _fileUtility.Received(1).DeleteContents(applicationFolder, Arg.Any<IEnumerable<string>>());
        //    Assert.IsTrue(receivedExclusions.Contains(backupFolder));
        //    Assert.IsTrue(receivedExclusions.Contains(updateTempFolder));
        //    Assert.IsTrue(receivedExclusions.Contains(dataFolder));
        //    Assert.IsTrue(receivedExclusions.Contains(updateEventLogFilePath));
        //    Assert.IsTrue(receivedExclusions.Contains(autoUpdaterFolder));
        //    Assert.AreEqual(5, receivedExclusions.Count());

        //}

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
