using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace SftpScheduler.BLL.Commands.Transfer
{
    public interface ISessionWrapper : IDisposable
    {
        Action<int>? ProgressCallback { get; set; }

        SessionOptions SessionOptions { get; set; }

        void Close();

        void GetFiles(string remotePath, string localPath, bool remove);

        IEnumerable<SftpScheduler.BLL.Models.RemoteFileInfo> ListDirectory(string remotePath);

        void Open();

        void PutFiles(string localPath, string remotePath);
    }

    public class SessionWrapper : ISessionWrapper
    {
        private readonly Session _session;

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

        public void GetFiles(string remotePath, string localPath, bool remove)
        {
            TransferOperationResult result = _session.GetFiles(remotePath, localPath, remove);
            result.Check();
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

        public void Open() 
        { 
            _session.Open(this.SessionOptions);
        }

        public void PutFiles(string localPath, string remotePath)
        {
            TransferOperationResult transferResult = _session.PutFiles(localPath, remotePath, false);
            transferResult.Check();
        }

        private void FileTransferProgressEventHandler(object sender, FileTransferProgressEventArgs e)
        {
            if (ProgressCallback != null)
            {
                int progress = Convert.ToInt32(e.FileProgress * 100);
                ProgressCallback(progress);
            }

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
