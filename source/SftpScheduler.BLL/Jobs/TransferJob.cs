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
        private readonly ITransferCommand _transferCommand;
        private readonly ICreateJobLogCommand _createJobLogCommand;
        private readonly IUpdateJobLogCompleteCommand _updateJobLogCompleteCommand;

        public TransferJob(ILogger<TransferJob> logger
            , IDbContextFactory dbContextFactory
            , ITransferCommand transferCommand
            , ICreateJobLogCommand createJobLogCommand
            , IUpdateJobLogCompleteCommand updateJobLogCompleteCommand
            ) 
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _transferCommand = transferCommand;
            _createJobLogCommand = createJobLogCommand;
            _updateJobLogCompleteCommand = updateJobLogCompleteCommand;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            int jobId = JobUtils.GetJobIdFromKeyName(context.JobDetail.Key.Name);
            string jobStatus = JobStatus.Success;
            string? errorMessage = null;
            int jobLogId = 0;
            _logger.LogInformation("Starting job {JobId}", jobId);

            // create the result record
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {

                dbContext.BeginTransaction();
                JobLogEntity jobLog = await _createJobLogCommand.ExecuteAsync(dbContext, jobId);
                jobLogId = jobLog.Id;
                dbContext.Commit();
                _logger.LogInformation("Created job log record for job {JobId}", jobId);
            }


            // execute the transfer
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                try
                {
                    _transferCommand.Execute(dbContext, jobId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Transfer of job {JobId} failed", jobId);
                    jobStatus = JobStatus.Failed;
                    errorMessage = ex.Message;
                }
            }

            // update the job log record
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                dbContext.BeginTransaction();
                await _updateJobLogCompleteCommand.ExecuteAsync(dbContext, jobLogId, 100, jobStatus, errorMessage);
                dbContext.Commit();
                _logger.LogInformation("Marked job log record for job {JobId} as complete, with status: '{JobStatus}'", jobId, jobStatus);
            }

        }
    }
}
