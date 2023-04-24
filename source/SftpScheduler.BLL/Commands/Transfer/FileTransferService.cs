using Microsoft.Extensions.Logging;
using SftpScheduler.BLL.Utility.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SftpScheduler.BLL.Commands.Transfer
{
    public interface IFileTransferService
    {
        void DownloadFiles(ISessionWrapper sessionWrapper, IEnumerable<string> filesToDownload, string localPath, bool deleteAfterDownload);

        IEnumerable<string> DownloadFilesAvailable(ISessionWrapper sessionWrapper, string remotePath);

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

        public void DownloadFiles(ISessionWrapper sessionWrapper, IEnumerable<string> filesToDownload
            , string localPath, bool deleteAfterDownload)
        {
            foreach (var remotePath in filesToDownload)
            {
                string fileName = Path.GetFileName(remotePath);
                string localFilePath = Path.Combine(localPath, fileName);
                sessionWrapper.GetFiles(remotePath, localFilePath, deleteAfterDownload);

                _logger.LogInformation("Downloaded file {remotePath} to {localPath}", remotePath, localFilePath);

            }
        }

        public IEnumerable<string> DownloadFilesAvailable(ISessionWrapper sessionWrapper, string remotePath)
        {
            IEnumerable<SftpScheduler.BLL.Models.RemoteFileInfo> remoteFiles = sessionWrapper.ListDirectory(remotePath);
            return remoteFiles.Where(x => !x.IsDirectory).Select(x => x.FullName);
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

                string destFilePath = GetUniqueFileName(folder, fileName, extension);
                _fileWrap.Move(file, destFilePath);
                _logger.LogInformation("File {file} uploaded and renamed to {destFilePath}", file, destFilePath);
            }
        }

        private string GetUniqueFileName(string? folder, string fileName, string extension)
        {
            string destFilePath = $"{folder}\\{fileName}.uploaded{extension}";
            int fileSuffix = 1;

            while (_fileWrap.Exists(destFilePath))
            {
                destFilePath = $"{folder}\\{fileName}.uploaded_{fileSuffix}{extension}";
                fileSuffix++;
            }

            return destFilePath;
        }
    }
}
