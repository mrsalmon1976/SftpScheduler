using Quartz;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Utility;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Commands.Job
{
    public class DeleteJobCommand
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public DeleteJobCommand(ISchedulerFactory schedulerFactory) 
        {
            _schedulerFactory = schedulerFactory;
        }

        public virtual async Task<bool> ExecuteAsync(IDbContext dbContext, int jobId)
        {
            const string sqlJobResult = @"DELETE FROM JobResult WHERE JobId = @JobId";
            await dbContext.ExecuteNonQueryAsync(sqlJobResult, new { JobId = jobId });

            const string sqlJob = @"DELETE FROM Job WHERE Id = @Id";
            await dbContext.ExecuteNonQueryAsync(sqlJob, new { Id = jobId });

            IScheduler scheduler = await _schedulerFactory.GetScheduler();

            TriggerKey triggerKey = new TriggerKey(TransferJob.GetTriggerKeyName(jobId), TransferJob.DefaultGroup);
            return await scheduler.UnscheduleJob(triggerKey);
        }
    }
}
