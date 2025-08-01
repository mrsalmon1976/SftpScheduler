using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.Common;
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
        private readonly IUploadCompressionService _uploadCompressionService;
        private readonly IDirectoryUtility _dirUtility;
        private readonly IFileUtility _fileUtility;
        private readonly IDateTimeProvider _dateTimeProvider;

        public const string UploadNamePattern = "^*.uploaded_?\\d?\\d?\\d?$";

        public FileTransferService(ILogger<FileTransferService> logger
            , ICreateJobFileLogCommand createJobFileLogCommand
            , IUploadCompressionService uploadCompressionService
            , IDirectoryUtility dirUtility
            , IFileUtility fileUtility
            , IDateTimeProvider dateTimeProvider)
        {
            _logger = logger;
			_createJobFileLogCommand = createJobFileLogCommand;
            _uploadCompressionService = uploadCompressionService;
            _dirUtility = dirUtility;
            _fileUtility = fileUtility;
            _dateTimeProvider = dateTimeProvider;
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
                DateTime startDate = _dateTimeProvider.Now;

				// get the files - note that if options.DeleteAfterDownload is set to true, the file will automatically be deleted by the client
				int downloadedCount = sessionWrapper.GetFiles(remoteFile.FullName, localFilePath, options);
                if (downloadedCount < 1)
                {
                    _logger.LogInformation("Remote file {remoteFileName} not downloaded: does not match mask or other download option(s)", remoteFile.FullName);
                    continue;
                }
                _logger.LogInformation("Downloaded file {remoteFileName} to {localPath}", remoteFile.FullName, localFilePath);

                // archive file remotely
                if (!options.DeleteAfterDownload)
                {
                    string archivePath = sessionWrapper.GetUniquePath(options.RemoteArchivePath ?? "/", fileName);
                    sessionWrapper.MoveFile(remoteFile.FullName, archivePath);
                    _logger.LogInformation("Remote file moved to {archivePath}", archivePath);
                }

                // make local copies, if any
                foreach (string localCopyPath in options.LocalCopyPaths)
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(localFilePath);
                    string extension = Path.GetExtension(localFilePath);
                    string fileCopyPath = GetUniqueFileName(localCopyPath, fileNameWithoutExt, String.Empty, extension);
                    _fileUtility.Copy(localFilePath, fileCopyPath);
                }

				// log the transfer
				JobFileLogEntity fileLog = new JobFileLogEntity();
				fileLog.JobId = options.JobId;
				fileLog.StartDate = startDate;
				fileLog.EndDate = _dateTimeProvider.Now;
                fileLog.FileLength = _fileUtility.GetFileSize(localFilePath);
                fileLog.FileName = fileName;
                _createJobFileLogCommand.ExecuteAsync(dbContext, fileLog);
			}
		}

        public IEnumerable<string> UploadFilesAvailable(string localPath)
        {
            IEnumerable<string> files = _dirUtility.EnumerateFiles(localPath);
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
            foreach (string filePath in options.LocalFilePaths)
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
				string fileName = Path.GetFileName(filePath);
				string extension = Path.GetExtension(filePath);
                string? folder = Path.GetDirectoryName(filePath);
                long fileSize = _fileUtility.GetFileSize(filePath);
                DateTime startDate = _dateTimeProvider.Now;

                CompressedFileInfo compressedFileInfo = _uploadCompressionService.PrepareUploadFile(filePath, options.CompressionMode);
                string fileToUpload = (compressedFileInfo.CompressedFilePath ?? filePath);
				int uploadedCount = sessionWrapper.PutFiles(fileToUpload, options);
                _uploadCompressionService.RemoveCompressedFile(compressedFileInfo);

                if (uploadedCount < 1)
                {
                    _logger.LogInformation("File {file} not uploaded: does not match mask or other upload option(s)", fileToUpload);
                    continue;
                }

                // if a prefix has been set, update the file name with the prefix
                if (!String.IsNullOrEmpty(options.LocalPrefix))
                {
                    string prefix = options.LocalPrefix ?? String.Empty;
                    const string format = "0#";
                    prefix = prefix.Replace("{YEAR}", _dateTimeProvider.Now.Year.ToString(), StringComparison.OrdinalIgnoreCase);
                    prefix = prefix.Replace("{MONTH}", _dateTimeProvider.Now.Month.ToString(format), StringComparison.OrdinalIgnoreCase);
                    prefix = prefix.Replace("{DAY}", _dateTimeProvider.Now.Day.ToString(format), StringComparison.OrdinalIgnoreCase);
                    prefix = prefix.Replace("{HOUR}", _dateTimeProvider.Now.Hour.ToString(format), StringComparison.OrdinalIgnoreCase);
                    prefix = prefix.Replace("{MINUTE}", _dateTimeProvider.Now.Minute.ToString(format), StringComparison.OrdinalIgnoreCase);
                    prefix = prefix.Replace("{SECOND}", _dateTimeProvider.Now.Second.ToString(format), StringComparison.OrdinalIgnoreCase);
                    fileNameWithoutExt = $"{prefix}{fileNameWithoutExt}";
                }

                // if we are not moving the file out, then rename with a ".uploaded" so we don't try and upload again
                string destFilePath = String.Empty;
                if (!String.IsNullOrEmpty(options.LocalArchivePath) && _dirUtility.Exists(options.LocalArchivePath))
                {
                    destFilePath = GetUniqueFileName(options.LocalArchivePath, fileNameWithoutExt, "", extension);
                }
                else
                {
                    destFilePath = GetUniqueFileName(folder, fileNameWithoutExt, ".uploaded", extension);
                }
                _fileUtility.Move(filePath, destFilePath, false);
                _logger.LogInformation("File {file} uploaded and renamed to {destFilePath}", filePath, destFilePath);

                // log the transfer
                JobFileLogEntity fileLog = new JobFileLogEntity();
				fileLog.JobId = options.JobId;
				fileLog.StartDate = startDate;
				fileLog.EndDate = _dateTimeProvider.Now;
				fileLog.FileLength = fileSize;
				fileLog.FileName = fileName;
				_createJobFileLogCommand.ExecuteAsync(dbContext, fileLog);

			}
		}

        private string GetUniqueFileName(string? folder, string fileName, string fileNameSuffix, string extension)
        {
            string destFilePath = $"{folder}\\{fileName}{fileNameSuffix}{extension}";
            int fileSuffix = 1;

            while (_fileUtility.Exists(destFilePath))
            {
                destFilePath = $"{folder}\\{fileName}{fileNameSuffix}_{fileSuffix}{extension}";
                fileSuffix++;
            }

            return destFilePath;
        }
    }
}
