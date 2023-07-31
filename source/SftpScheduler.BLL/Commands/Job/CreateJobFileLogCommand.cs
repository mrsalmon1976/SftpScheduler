using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SftpScheduler.BLL.Commands.Job
{
    public interface ICreateJobFileLogCommand
    {
        Task<JobFileLogEntity> ExecuteAsync(IDbContext dbContext, JobFileLogEntity jobFileLog);
    }

    public class CreateJobFileLogCommand : ICreateJobFileLogCommand
	{
        public virtual async Task<JobFileLogEntity> ExecuteAsync(IDbContext dbContext, JobFileLogEntity jobFileLog)
        {
            string sql = @"INSERT INTO JobFileLog (JobId, FileName, FileLength, StartDate, EndDate) VALUES (@JobId, @FileName, @FileLength, @StartDate, @EndDate)";
            await dbContext.ExecuteNonQueryAsync(sql, jobFileLog);

            sql = @"select last_insert_rowid()";
			jobFileLog.Id = await dbContext.ExecuteScalarAsync<int>(sql);

            return jobFileLog;

        }

    }
}
