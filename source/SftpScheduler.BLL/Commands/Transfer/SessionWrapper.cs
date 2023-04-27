﻿using SftpScheduler.BLL.Models;
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

        void MoveFile(string sourcePath, string targetPath);

        void Open();

        void PutFiles(string localPath, string remotePath);

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

        public void MoveFile(string sourcePath, string targetPath)
        {
            _session.MoveFile(sourcePath, targetPath);
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

        public string ScanFingerprint(string algorithm)
        {
            //SessionOptions sessionOptions = new SessionOptions
            //{
            //    Protocol = Protocol.Sftp,   
            //    HostName = hostName,
            //    PortNumber = port ?? 22
            //};

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
