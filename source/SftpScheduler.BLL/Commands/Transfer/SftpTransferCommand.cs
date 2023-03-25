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
    public class SftpTransferCommand : ITransferCommand
    {
        private readonly ILogger<SftpTransferCommand> _logger;
        private readonly IDbContextFactory _dbContextFactory;
        private readonly JobRepository _jobRepository;
        private readonly HostRepository _hostRepository;
        private readonly IDbContext _dbContext;

        public SftpTransferCommand(ILogger<SftpTransferCommand> logger, IDbContextFactory dbContextFactory, JobRepository jobRepository, HostRepository hostRepository) 
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _jobRepository = jobRepository;
            _hostRepository = hostRepository;

            _dbContext = _dbContextFactory.GetDbContext(); 
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        public void Execute(int jobId)
        {
            JobEntity jobEntity = _jobRepository.GetByIdAsync(_dbContext, jobId).GetAwaiter().GetResult();
            HostEntity hostEntity = _hostRepository.GetByIdAsync(_dbContext, jobEntity.HostId).GetAwaiter().GetResult();

            // Setup session options
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = hostEntity.Host,
                UserName = hostEntity.Username,
                Password = hostEntity.Password,
                SshHostKeyPolicy = SshHostKeyPolicy.GiveUpSecurityAndAcceptAny  // TODO!  This needs to be removed!
                //SshHostKeyFingerprint = "ssh-rsa 2048 xxxxxxxxxxx..."
            };

            using (Session session = new Session())
            {
                session.FileTransferProgress += SessionFileTransferProgress;

                try
                {
                    // Connect
                    session.Open(sessionOptions);

                    if (jobEntity.Type == JobType.Download)
                    {
                        this.DownloadFiles(session, jobEntity);
                    }
                    else
                    {
                        this.UploadFiles(session, jobEntity);
                    }

                    session.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
                finally
                {
                    if (session != null)
                    {
                        session.FileTransferProgress -= SessionFileTransferProgress;
                    }
                }
            }

        }

        private void SessionFileTransferProgress(object sender, FileTransferProgressEventArgs e)
        {
        }

        private void DownloadFiles(Session session, JobEntity jobEntity)
        {

        }

        private void UploadFiles(Session session, JobEntity jobEntity)
        {
            IEnumerable<string> files = Directory.EnumerateFiles(jobEntity.LocalPath);

            TransferOptions transferOptions = new TransferOptions();
            transferOptions.TransferMode = TransferMode.Automatic;

            foreach (string file in files) 
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file);
                string? folder = Path.GetDirectoryName(file);

                if (fileName.EndsWith(".uploaded", StringComparison.CurrentCultureIgnoreCase))
                {
                    _logger.LogDebug($"Ignoring file {file} - already uploaded");
                    continue;
                }

                var transferResult = session.PutFiles(file, jobEntity.RemotePath, false, transferOptions);
                transferResult.Check();

                string destFilePath = $"{folder}\\{fileName}.uploaded{extension}";
                File.Move(file, destFilePath);
                _logger.LogInformation($"File {file} uploaded and renamed to {destFilePath}");
            }
        }

    }
}
