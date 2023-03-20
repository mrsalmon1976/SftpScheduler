using Microsoft.Extensions.Logging;
using Quartz;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Jobs
{
    public class TransferJob : IJob
    {
        public const string DefaultGroup = "TransferJob";
        private readonly ILogger<TransferJob> _logger;
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ITransferCommandFactory _transferCommandFactory;
        private readonly ICreateJobResultCommand _createJobResultCommand;
        private readonly IUpdateJobResultCompleteCommand _updateJobResultCompleteCommand;

        public TransferJob(ILogger<TransferJob> logger
            , IDbContextFactory dbContextFactory
            , ITransferCommandFactory transferCommandFactory
            , ICreateJobResultCommand createJobResultCommand
            , IUpdateJobResultCompleteCommand updateJobResultCompleteCommand
            ) 
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _transferCommandFactory = transferCommandFactory;
            _createJobResultCommand = createJobResultCommand;
            _updateJobResultCompleteCommand = updateJobResultCompleteCommand;
        }

        public static string GetJobKeyName(int jobId)
        {
            return $"Job.{jobId}";
        }

        public static int GetJobIdFromKeyName(string keyName)
        {
            return Convert.ToInt32(keyName.Replace("Job.", ""));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            int jobId = GetJobIdFromKeyName(context.JobDetail.Key.Name);
            JobResultEntity jobResult = null;
            _logger.LogInformation($"Starting job {jobId}");

            // create the result record
            using (IDbContext dbContext = _dbContextFactory.GetDbContext()) 
            {
                dbContext.BeginTransaction();
                jobResult = await _createJobResultCommand.ExecuteAsync(dbContext, jobId);
                dbContext.Commit();
                _logger.LogInformation($"Created job result record for job {jobId}");
            }

            string jobStatus = JobResult.Success;
            string? errorMessage = null;

            // execute the transfer
            using (ITransferCommand transferCommand = _transferCommandFactory.CreateTransferCommand())
            {
                try
                {
                    transferCommand.Execute(jobId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    jobStatus = JobResult.Failure;
                    errorMessage = ex.Message;
                }
            }

            // update the record
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                dbContext.BeginTransaction();
                await _updateJobResultCompleteCommand.ExecuteAsync(dbContext, jobResult.Id, 100, jobStatus, errorMessage);
                dbContext.Commit();
                _logger.LogInformation($"Marked job result record for job {jobId} as complete, status {jobStatus}");
            }

        }
    }
}
