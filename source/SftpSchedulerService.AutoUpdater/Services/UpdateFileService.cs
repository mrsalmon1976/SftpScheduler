using SftpScheduler.BLL.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.AutoUpdater.Services
{
    public interface IUpdateFileService
    {
        //Task Backup();

        //Task CopyNewVersionFiles(string newVersionZipFileName);

        //Task DeleteCurrentVersionFiles();

        void DeleteUpdateTempFolder(string updateTempFolder);

        void EnsureEmptyUpdateTempFolderExists(string updateTempFolder);

        //Task ExtractReleasePackage(string filePath, string extractFolder);

    }

    public class UpdateFileService : IUpdateFileService
    {
        private readonly IFileUtility _fileUtility;
        private readonly IDirectoryUtility _dirUtility;

        public UpdateFileService(IFileUtility fileUtility, IDirectoryUtility dirUtility)
        {
            _fileUtility = fileUtility;
            _dirUtility = dirUtility;
        }

        //public async Task Backup()
        //{
        //    await Task.Run(() => {
        //        _fileUtility.DeleteDirectoryRecursive(_updateLocationService.BackupFolder);
        //        string[] exclusions = { _updateLocationService.BackupFolder, _updateLocationService.UpdateTempFolder };
        //        _fileUtility.CopyRecursive(_updateLocationService.ApplicationFolder, _updateLocationService.BackupFolder, exclusions);
        //    });
        //}

        //public async Task CopyNewVersionFiles(string newVersionZipFileName)
        //{
        //    await Task.Run(() => {
        //        _fileUtility.CopyRecursive(_updateLocationService.UpdateTempFolder, _updateLocationService.ApplicationFolder);
        //    });
        //}

        //public async Task DeleteCurrentVersionFiles()
        //{
        //    await Task.Run(() => {

        //        string[] exclusions = {
        //            _updateLocationService.BackupFolder
        //            , _updateLocationService.UpdateTempFolder
        //            , _updateLocationService.DataFolder
        //            , _updateLocationService.UpdateEventLogFilePath
        //            , _updateLocationService.AutoUpdaterFolder
        //        };
        //        _fileUtility.DeleteContents(_updateLocationService.ApplicationFolder, exclusions);
        //    });
        //}

        public void DeleteUpdateTempFolder(string updateTempFolder)
        {
            if (_dirUtility.Exists(updateTempFolder))
            {
                _dirUtility.Delete(updateTempFolder, true);
            }
        }


        public void EnsureEmptyUpdateTempFolderExists(string updateTempFolder)
        {
            this.DeleteUpdateTempFolder(updateTempFolder);
            _dirUtility.Create(updateTempFolder);
        }

        //public async Task ExtractReleasePackage(string filePath, string extractFolder)
        //{
        //    await Task.Run(() => {
        //        _fileUtility.ExtractZipFile(filePath, extractFolder);

        //        // rename all the autoupdater files with a .temp extension otherwise we will fail to overwrite
        //        string autoUpdateFolder = Path.Combine(extractFolder, UpdateConstants.AutoUpdaterFolderName);
        //        if (_fileUtility.DirectoryExists(autoUpdateFolder))
        //        {
        //            string[] files = _fileUtility.GetFiles(autoUpdateFolder, SearchOption.AllDirectories);
        //            foreach (string f in files)
        //            {
        //                string newName = $"{f}{UpdateConstants.AutoUpdaterNewFileExtension}";
        //                _fileUtility.MoveFile(f, newName, true);
        //            }
        //        }

        //        // delete the zip file
        //        _fileUtility.DeleteFile(filePath);

        //    });
        //}
    }
}
