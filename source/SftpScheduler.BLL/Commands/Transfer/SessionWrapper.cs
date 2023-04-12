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

        void Open();

        void PutFiles(string localPath, string remotePath, TransferOptions options);
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

        public void Open() 
        { 
            _session.Open(this.SessionOptions);
        }

        public void PutFiles(string localPath, string remotePath, TransferOptions options)
        {
            TransferOperationResult transferResult = _session.PutFiles(localPath, remotePath, false, options);
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
