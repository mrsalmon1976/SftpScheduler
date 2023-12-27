using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WinSCP;

namespace SftpScheduler.BLL.Commands.Transfer
{
    public interface IFileTransferService
    {
        void DownloadFiles(ISessionWrapper sessionWrapper, IDbContext dbContext, DownloadOptions options);

        void UploadFiles(ISessionWrapper sessionWrapper, IDbContext dbContext, UploadOptions options);

        IEnumerable<string> UploadFilesAvailable(string localPath);
    }

    public class FileTransferService : IFileTransferService
    {
        private readonly ILogger<FileTransferService> _logger;
		private readonly ICreateJobFileLogCommand _createJobFileLogCommand;
		private readonly IDirectoryUtility _directoryWrap;
        private readonly IFileUtility _fileWrap;

        public const string UploadNamePattern = "^*.uploaded_?\\d?\\d?\\d?$";

        public FileTransferService(ILogger<FileTransferService> logger, ICreateJobFileLogCommand createJobFileLogCommand, IDirectoryUtility directoryWrap, IFileUtility fileWrap)
        {
            _logger = logger;
			_createJobFileLogCommand = createJobFileLogCommand;
			_directoryWrap = directoryWrap;
            _fileWrap = fileWrap;
        }

        public void DownloadFiles(ISessionWrapper sessionWrapper, IDbContext dbContext, DownloadOptions options)
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
                DateTime startDate = DateTime.Now;

				// get the files - note the DeleteAfterDownload option is used here so files are removed by WinSCP
				sessionWrapper.GetFiles(remoteFile.FullName, localFilePath, options.DeleteAfterDownload, options.FileMask);
                _logger.LogInformation("Downloaded file {remoteFileName} to {localPath}", remoteFile.FullName, localFilePath);

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

				// log the transfer
				JobFileLogEntity fileLog = new JobFileLogEntity();
				fileLog.JobId = options.JobId;
				fileLog.StartDate = startDate;
				fileLog.EndDate = DateTime.Now;
                fileLog.FileLength = _fileWrap.GetFileSize(localFilePath);
                fileLog.FileName = fileName;
                _createJobFileLogCommand.ExecuteAsync(dbContext, fileLog);
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

        public void UploadFiles(ISessionWrapper sessionWrapper, IDbContext dbContext, UploadOptions options)
        {
            foreach (string file in options.LocalFilePaths)
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
				string fileName = Path.GetFileName(file);
				string extension = Path.GetExtension(file);
                string? folder = Path.GetDirectoryName(file);
                DateTime startDate = DateTime.Now;


				sessionWrapper.PutFiles(file, options.RemotePath, options.RestartOnFailure, options.FileMask);

                string destFilePath = GetUniqueFileName(folder, fileNameWithoutExt, ".uploaded", extension);
                _fileWrap.Move(file, destFilePath, false);
                _logger.LogInformation("File {file} uploaded and renamed to {destFilePath}", file, destFilePath);

				// log the transfer
				JobFileLogEntity fileLog = new JobFileLogEntity();
				fileLog.JobId = options.JobId;
				fileLog.StartDate = startDate;
				fileLog.EndDate = DateTime.Now;
				fileLog.FileLength = _fileWrap.GetFileSize(destFilePath);
				fileLog.FileName = fileName;
				_createJobFileLogCommand.ExecuteAsync(dbContext, fileLog);

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
