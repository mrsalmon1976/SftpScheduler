﻿using Microsoft.Extensions.Logging;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

#pragma warning disable CS8629 // Nullable value type may be null.

namespace SftpScheduler.BLL.Commands.Transfer
{
    public interface ITransferCommand
    {
        void Execute(IDbContext dbContext, int jobId);
    }

    public class TransferCommand : ITransferCommand
    {
        private readonly ILogger<TransferCommand> _logger;
        private readonly ISessionWrapperFactory _sessionWrapperFactory;
        private readonly IFileTransferService _fileTransferService;
        private readonly JobRepository _jobRepository;
        private readonly HostRepository _hostRepository;

        public TransferCommand(ILogger<TransferCommand> logger
            , ISessionWrapperFactory sessionWrapperFactory
            , IFileTransferService fileTransferService
            , JobRepository jobRepository
            , HostRepository hostRepository) 
        {
            _logger = logger;
            _sessionWrapperFactory = sessionWrapperFactory;
            _fileTransferService = fileTransferService;
            _jobRepository = jobRepository;
            _hostRepository = hostRepository;
        }

        public void Execute(IDbContext dbContext, int jobId)
        {
            JobEntity jobEntity = _jobRepository.GetByIdAsync(dbContext, jobId).GetAwaiter().GetResult();
            HostEntity hostEntity = _hostRepository.GetByIdOrDefaultAsync(dbContext, jobEntity.HostId).GetAwaiter().GetResult();
            List<string> filesToTransfer = new List<string>();

            // for upload jobs, check if there are any files first to avoid the overhead of a connection
            if (jobEntity.Type == JobType.Upload)
            {
                filesToTransfer.AddRange(_fileTransferService.UploadFilesAvailable(jobEntity.LocalPath));
                if (filesToTransfer.Count == 0)
                {
                    _logger.LogInformation("No files available for upload for job {JobId}", jobId);
                    return;
                }
            }

            // we have to connect now
            using (ISessionWrapper sessionWrapper = _sessionWrapperFactory.CreateSession(hostEntity))
            {
                try
                {
                    // Connect
                    sessionWrapper.ProgressCallback = this.ProgressCallback;
                    sessionWrapper.Open();

                    if (jobEntity.Type == JobType.Download)
                    {
                        DownloadOptions options = this.BuildDownloadOptions(jobEntity);
                        _fileTransferService.DownloadFiles(sessionWrapper, dbContext, options);
                    }
                    else
                    {
                        UploadOptions options = new UploadOptions(jobId, filesToTransfer, jobEntity.RemotePath, jobEntity.RestartOnFailure);
                        _fileTransferService.UploadFiles(sessionWrapper, dbContext, options);
                    }

                }
                finally
                {
                    sessionWrapper.ProgressCallback = null;
                    sessionWrapper.Close();
                }
            }

        }

        private DownloadOptions BuildDownloadOptions(JobEntity jobEntity)
        {
            DownloadOptions options = new DownloadOptions(jobEntity.Id, jobEntity.LocalPath, jobEntity.RemotePath) 
            {
                DeleteAfterDownload = jobEntity.DeleteAfterDownload,
                RemoteArchivePath = jobEntity.RemoteArchivePath
            };
            options.LocalCopyPaths.AddRange((jobEntity.LocalCopyPaths ?? String.Empty).Split(';').Where(x => x.Length > 0));
            return options;
        }


        private void ProgressCallback(int progress)
        {
            _logger.LogInformation("PROGRESS------> " + progress.ToString());
        }


    }
}
