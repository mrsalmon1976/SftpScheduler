using Microsoft.Extensions.Logging;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Utility.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace SftpScheduler.BLL.Commands.Transfer
{
    public interface IFileTransferService
    {
        void DownloadFiles(ISessionWrapper sessionWrapper, string localPath);

        void UploadFiles(ISessionWrapper sessionWrapper, IEnumerable<string> filesToUpload, string remotePath);

        IEnumerable<string> UploadFilesAvailable(string localPath);
    }

    public class FileTransferService : IFileTransferService
    {
        private readonly ILogger<FileTransferService> _logger;
        private readonly IDirectoryWrap _directoryWrap;
        private readonly IFileWrap _fileWrap;

        public FileTransferService(ILogger<FileTransferService> logger, IDirectoryWrap directoryWrap, IFileWrap fileWrap)
        {
            _logger = logger;
            _directoryWrap = directoryWrap;
            _fileWrap = fileWrap;
        }

        public void DownloadFiles(ISessionWrapper sessionWrapper, string localPath)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> UploadFilesAvailable(string localPath)
        {
            IEnumerable<string> files = _directoryWrap.EnumerateFiles(localPath);
            List<string> newFiles = new List<string>();

            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                if (fileName.EndsWith(".uploaded", StringComparison.CurrentCultureIgnoreCase))
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
            TransferOptions transferOptions = new TransferOptions();
            transferOptions.TransferMode = TransferMode.Automatic;

            foreach (string file in filesToUpload)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file);
                string? folder = Path.GetDirectoryName(file);

                sessionWrapper.PutFiles(file, remotePath, transferOptions);

                string destFilePath = $"{folder}\\{fileName}.uploaded{extension}";
                _fileWrap.Move(file, destFilePath);
                _logger.LogInformation("File {file} uploaded and renamed to {destFilePath}", file, destFilePath);
            }
        }
    }
}
