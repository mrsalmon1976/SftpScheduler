using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SftpScheduler.BLL.Commands.Job
{
    public interface ICreateJobLogCommand
    {
        Task<JobLogEntity> ExecuteAsync(IDbContext dbContext, int jobId);
    }

    public class CreateJobLogCommand : ICreateJobLogCommand
    {
        public virtual async Task<JobLogEntity> ExecuteAsync(IDbContext dbContext, int jobId)
        {
            JobLogEntity jobLog = new JobLogEntity();
            jobLog.JobId = jobId;
            jobLog.StartDate = DateTime.Now;
            jobLog.Status = JobStatus.InProgress;
            string sql = @"INSERT INTO JobLog (JobId, StartDate, Progress, Status) VALUES (@JobId, DATETIME(), @Progress, @Status)";
            await dbContext.ExecuteNonQueryAsync(sql, jobLog);

            sql = @"select last_insert_rowid()";
            jobLog.Id = await dbContext.ExecuteScalarAsync<int>(sql);

            return jobLog;

        }

    }
}
