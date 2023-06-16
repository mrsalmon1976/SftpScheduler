using Quartz;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Commands.Job
{
    public interface IDeleteJobCommand
    {
        Task<bool> ExecuteAsync(IDbContext dbContext, int jobId);
    }

    public class DeleteJobCommand : IDeleteJobCommand
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public DeleteJobCommand(ISchedulerFactory schedulerFactory) 
        {
            _schedulerFactory = schedulerFactory;
        }

        public virtual async Task<bool> ExecuteAsync(IDbContext dbContext, int jobId)
        {
            const string sqlJobLog = @"DELETE FROM JobLog WHERE JobId = @JobId";
            await dbContext.ExecuteNonQueryAsync(sqlJobLog, new { JobId = jobId });

            const string sqlJob = @"DELETE FROM Job WHERE Id = @Id";
            await dbContext.ExecuteNonQueryAsync(sqlJob, new { Id = jobId });

            IScheduler scheduler = await _schedulerFactory.GetScheduler();

            JobKey jobKey = new JobKey(JobUtils.GetJobKeyName(jobId), TransferJob.DefaultGroup);

            return await scheduler.DeleteJob(jobKey);

        }
    }
}
