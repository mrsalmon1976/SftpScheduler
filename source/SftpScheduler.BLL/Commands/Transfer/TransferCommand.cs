using Microsoft.Extensions.Logging;
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
            List<string> filesToUpload = new List<string>();

            // for upload jobs, check if there are any files first to avoid the overhead of a connection
            if (jobEntity.Type == JobType.Upload)
            {
                filesToUpload.AddRange(_fileTransferService.UploadFilesAvailable(jobEntity.LocalPath));
                if (filesToUpload.Count == 0)
                {
                    _logger.LogInformation("No files available for upload for job {JobId}", jobId);
                    return;
                }
            }

            // we have to connect now
            using (ISessionWrapper sessionWrapper = _sessionWrapperFactory.CreateSession(hostEntity, jobEntity))
            {
                try
                {
                    // Connect
                    sessionWrapper.ProgressCallback = this.ProgressCallback;
                    sessionWrapper.Open();

                    if (jobEntity.Type == JobType.Download)
                    {
                        _fileTransferService.DownloadFiles(sessionWrapper, "");
                    }
                    else
                    {
                        _fileTransferService.UploadFiles(sessionWrapper, filesToUpload, jobEntity.RemotePath);
                    }

                }
                finally
                {
                    sessionWrapper.ProgressCallback = null;
                    sessionWrapper.Close();
                }
            }

        }


        private void ProgressCallback(int progress)
        {
            _logger.LogInformation("PROGRESS------> " + progress.ToString());
        }


    }
}
