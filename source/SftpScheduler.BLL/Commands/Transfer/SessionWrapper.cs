using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WinSCP;

namespace SftpScheduler.BLL.Commands.Transfer
{
    public interface ISessionWrapper : IDisposable
    {
        Action<int>? ProgressCallback { get; set; }

        SessionOptions SessionOptions { get; set; }

        void Close();

        int GetFiles(string remotePath, string localPath, DownloadOptions downloadOptions);

        string GetUniquePath(string remoteFolder, string remoteFileName);

        IEnumerable<SftpScheduler.BLL.Models.RemoteFileInfo> ListDirectory(string remotePath);

        void MoveFile(string sourcePath, string targetPath);

        void Open();

        int PutFiles(string localFilePath, UploadOptions uploadOptions);

        string ScanFingerprint(string algorithm);
    }

    public class SessionWrapper : ISessionWrapper
    {
        private readonly Session _session;

        public const string AlgorithmSha256 = "SHA-256";
        public const string AlgorithmMd5 = "MD5";

        public SessionWrapper(SessionOptions sessionOptions)
        {
            this._session = new Session();
            this._session.FileTransferProgress += FileTransferProgressEventHandler;
            this.SessionOptions = sessionOptions;
        }

        public Action<int>? ProgressCallback { get; set; }

        public SessionOptions SessionOptions { get; set; }

        public void Close()
        {
            if (_session != null && _session.Opened)
            {
                _session.Close();
            }
        }

        public int GetFiles(string remotePath, string localPath, DownloadOptions downloadOptions)
        {
            TransferOptions transferOptions = new TransferOptions();
            transferOptions.TransferMode = MapTransferMode(downloadOptions.TransferMode);
            if (!String.IsNullOrEmpty(downloadOptions.FileMask))
            {
                transferOptions.FileMask = downloadOptions.FileMask;
            }
            TransferOperationResult result = _session.GetFiles(remotePath, localPath, downloadOptions.DeleteAfterDownload, transferOptions);
            result.Check();
            return result.Transfers.Count;
        }

        public string GetUniquePath(string remoteFolder, string remoteFileName) 
        {
            string folder = remoteFolder.TrimEnd('/');
            string resultPath = String.Concat(folder, '/', remoteFileName);

            string fileName = Path.GetFileNameWithoutExtension(remoteFileName);
            string fileExtension = Path.GetExtension(remoteFileName);

            // try and build a path with a number, but not too many times
            const int attempts = 10;
            for (int i = 1; i <= attempts; i++)
            {
                if (!_session.FileExists(resultPath))
                {
                    return resultPath;
                }

                resultPath = String.Concat(folder, '/', fileName, '_', i, fileExtension);
            }

            // unable to find a unique path with a number - just use a guid
            return String.Concat(folder, '/', fileName, '_', Guid.NewGuid().ToString(), fileExtension);

        }

        public IEnumerable<SftpScheduler.BLL.Models.RemoteFileInfo> ListDirectory(string remotePath)
        {
            RemoteDirectoryInfo remoteDirectoryInfo = _session.ListDirectory(remotePath);
            var remoteFiles = remoteDirectoryInfo.Files.Select(x => new SftpScheduler.BLL.Models.RemoteFileInfo()
            {
                IsDirectory = x.IsDirectory,
                Name = x.Name,
                FullName = x.FullName,
                Length = x.Length,
                LastWriteTime = x.LastWriteTime
            });

            return remoteFiles;
        }

        public void MoveFile(string sourcePath, string targetPath)
        {
            _session.MoveFile(sourcePath, targetPath);
        }

        public void Open() 
        {
            _session.Open(this.SessionOptions);

        }

        public int PutFiles(string localPath, UploadOptions uploadOptions)
        {
            TransferOptions transferOptions = new TransferOptions();
            transferOptions.PreserveTimestamp = uploadOptions.PreserveTimestamp;
            transferOptions.TransferMode = MapTransferMode(uploadOptions.TransferMode);
            if (uploadOptions.RestartOnFailure)
            {
                transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;
            }
            if (!String.IsNullOrEmpty(uploadOptions.FileMask))
            {
                transferOptions.FileMask = uploadOptions.FileMask;
            }

            TransferOperationResult transferResult = _session.PutFiles(localPath, uploadOptions.RemotePath, false, transferOptions);
            transferResult.Check();
            return transferResult.Transfers.Count;
        }

        public string ScanFingerprint(string algorithm)
        {
            return _session.ScanFingerprint(this.SessionOptions, algorithm);
        }

        private void FileTransferProgressEventHandler(object sender, FileTransferProgressEventArgs e)
        {
            if (ProgressCallback != null)
            {
                int progress = Convert.ToInt32(e.FileProgress * 100);
                ProgressCallback(progress);
            }

        }

        private WinSCP.TransferMode MapTransferMode(Data.TransferMode transferMode)
        {
            switch (transferMode)
            {
                case Data.TransferMode.Binary:
                    return WinSCP.TransferMode.Binary;
                case Data.TransferMode.Automatic:
                    return WinSCP.TransferMode.Automatic;
                case Data.TransferMode.Ascii:
                    return WinSCP.TransferMode.Ascii;
            }

            throw new NotSupportedException($"Unsupported transfermode {transferMode}");
        }

        public void Dispose()
        {
            if (_session != null)
            {
                _session.FileTransferProgress -= FileTransferProgressEventHandler;
                _session.Dispose();
            }
        }
    }
}
