using Microsoft.Extensions.Logging;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Utility.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WinSCP;

namespace SftpScheduler.BLL.Commands.Transfer
{
    public interface IFileTransferService
    {
        void DownloadFiles(ISessionWrapper sessionWrapper, DownloadOptions options);

        void UploadFiles(ISessionWrapper sessionWrapper, IEnumerable<string> filesToUpload, string remotePath);

        IEnumerable<string> UploadFilesAvailable(string localPath);
    }

    public class FileTransferService : IFileTransferService
    {
        private readonly ILogger<FileTransferService> _logger;
        private readonly IDirectoryWrap _directoryWrap;
        private readonly IFileWrap _fileWrap;

        public const string UploadNamePattern = "^*.uploaded_?\\d?\\d?\\d?$";

        public FileTransferService(ILogger<FileTransferService> logger, IDirectoryWrap directoryWrap, IFileWrap fileWrap)
        {
            _logger = logger;
            _directoryWrap = directoryWrap;
            _fileWrap = fileWrap;
        }

        public void DownloadFiles(ISessionWrapper sessionWrapper, DownloadOptions options)
        {
            IEnumerable<SftpScheduler.BLL.Models.RemoteFileInfo> remoteFiles = sessionWrapper.ListDirectory(options.RemotePath);
            IEnumerable<SftpScheduler.BLL.Models.RemoteFileInfo> filesToDownload = remoteFiles.Where(x => !x.IsDirectory);

            if (!filesToDownload.Any())
            {
                _logger.LogInformation("No files available for download job {jobId}", options.JobId);
                return;
            }

            foreach (var remoteFile in filesToDownload)
            {
                string fileName = remoteFile.Name;
                string localFilePath = Path.Combine(options.LocalPath, fileName);
                sessionWrapper.GetFiles(remoteFile.FullName, localFilePath, options.DeleteAfterDownload);
                _logger.LogInformation("Downloaded file {remoteFile} to {localPath}", remoteFile, localFilePath);

                // archive file remotely
                if (!options.DeleteAfterDownload)
                {
                    string archivePath = $"{options.RemoteArchivePath}{fileName}";
                    sessionWrapper.MoveFile(remoteFile.FullName, archivePath);
                    _logger.LogInformation("Remote file moved to {archivePath}", archivePath);
                }

                // make local copies, if any
                foreach (string localCopyPath in options.LocalCopyPaths)
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(localFilePath);
                    string extension = Path.GetExtension(localFilePath);
                    string fileCopyPath = GetUniqueFileName(localCopyPath, fileNameWithoutExt, String.Empty, extension);
                    _fileWrap.Copy(localFilePath, fileCopyPath);
                }
            }
        }

        public IEnumerable<string> UploadFilesAvailable(string localPath)
        {
            IEnumerable<string> files = _directoryWrap.EnumerateFiles(localPath);
            List<string> newFiles = new List<string>();
            Regex regEx = new Regex(UploadNamePattern, RegexOptions.IgnoreCase);

            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                if (regEx.IsMatch(fileName))
                {
                    _logger.LogDebug("File '{file}' skipped: already uploaded", file);
                    continue;
                }

                newFiles.Add(file);

            }

            return newFiles;
        }

        public void UploadFiles(ISessionWrapper sessionWrapper, IEnumerable<string> filesToUpload, string remotePath)
        {
            foreach (string file in filesToUpload)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file);
                string? folder = Path.GetDirectoryName(file);

                sessionWrapper.PutFiles(file, remotePath);

                string destFilePath = GetUniqueFileName(folder, fileName, ".uploaded", extension);
                _fileWrap.Move(file, destFilePath);
                _logger.LogInformation("File {file} uploaded and renamed to {destFilePath}", file, destFilePath);
            }
        }

        private string GetUniqueFileName(string? folder, string fileName, string fileNameSuffix, string extension)
        {
            string destFilePath = $"{folder}\\{fileName}{fileNameSuffix}{extension}";
            int fileSuffix = 1;

            while (_fileWrap.Exists(destFilePath))
            {
                destFilePath = $"{folder}\\{fileName}{fileNameSuffix}_{fileSuffix}{extension}";
                fileSuffix++;
            }

            return destFilePath;
        }
    }
}
