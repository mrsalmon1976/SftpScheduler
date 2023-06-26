using SftpScheduler.Common.IO;
using SftpScheduler.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerServiceUpdater.Services
{
    public interface IUpdateFileService
    {
        void Backup(UpdateLocationInfo updateLocationInfo);
        void CopyNewVersionFiles(UpdateLocationInfo updateLocationInfo);

        void DeleteCurrentVersionFiles(UpdateLocationInfo updateLocationInfo);

        void DeleteUpdateTempFolder(string updateTempFolder);

        void EnsureEmptyUpdateTempFolderExists(string updateTempFolder);

        void ExtractReleasePackage(string filePath, string extractFolder);

    }

    public class UpdateFileService : IUpdateFileService
    {
        private readonly IFileUtility _fileUtility;
        private readonly IDirectoryUtility _dirUtility;

        public const int MaxDeleteRetryCount = 10;

        public UpdateFileService(IFileUtility fileUtility, IDirectoryUtility dirUtility)
        {
            _fileUtility = fileUtility;
            _dirUtility = dirUtility;
        }

        public void Backup(UpdateLocationInfo updateLocationInfo)
        {
            string[] exclusions = { updateLocationInfo.BackupFolder, updateLocationInfo.UpdateTempFolder };

            _dirUtility.Delete(updateLocationInfo.BackupFolder, MaxDeleteRetryCount);
            _dirUtility.CopyRecursive(updateLocationInfo.ApplicationFolder, updateLocationInfo.BackupFolder, exclusions);
        }

        public void CopyNewVersionFiles(UpdateLocationInfo updateLocationInfo)
        {
            _dirUtility.CopyRecursive(updateLocationInfo.UpdateTempFolder, updateLocationInfo.ApplicationFolder);
        }

        public void DeleteCurrentVersionFiles(UpdateLocationInfo updateLocationInfo)
        {
            string[] exclusions = {
                updateLocationInfo.BackupFolder
                , updateLocationInfo.UpdateTempFolder
                , updateLocationInfo.DataFolder
                , updateLocationInfo.LogFolder
                , updateLocationInfo.UpdaterFolder
            };
            _dirUtility.DeleteContents(updateLocationInfo.ApplicationFolder, exclusions, MaxDeleteRetryCount);
        }

        public void DeleteUpdateTempFolder(string updateTempFolder)
        {
            if (_dirUtility.Exists(updateTempFolder))
            {
                _dirUtility.Delete(updateTempFolder, MaxDeleteRetryCount);
            }
        }


        public void EnsureEmptyUpdateTempFolderExists(string updateTempFolder)
        {
            this.DeleteUpdateTempFolder(updateTempFolder);
            _dirUtility.Create(updateTempFolder);
        }

        public void ExtractReleasePackage(string filePath, string extractFolder)
        {
            _fileUtility.ExtractZipFile(filePath, extractFolder);

            // rename all the autoupdater files with a .temp extension otherwise we will fail to overwrite
            string autoUpdateFolder = Path.Combine(extractFolder, UpdateConstants.UpdaterFolderName);
            if (_dirUtility.Exists(autoUpdateFolder))
            {
                string[] files = _dirUtility.GetFiles(autoUpdateFolder, SearchOption.AllDirectories);
                foreach (string f in files)
                {
                    string newName = $"{f}{UpdateConstants.UpdaterNewFileExtension}";
                    _fileUtility.Move(f, newName, true);
                }
            }

            // delete the zip file
            _fileUtility.Delete(filePath);
        }
    }
}
